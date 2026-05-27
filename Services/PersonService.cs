using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrpcCrudDemo.Data;
using GrpcCrudDemo.Models;

namespace GrpcCrudDemo.Services
{
    public class PersonService : GrpcCrudDemo.PersonService.PersonServiceBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PersonService> _logger;

        public PersonService(AppDbContext db, ILogger<PersonService> logger)
        {
            _db = db;
            _logger = logger;
        }

        private void ValidatePersonFields(string firstName, string lastName, string nationalCode)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "FirstName and LastName are required."));

            if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10 || !nationalCode.All(char.IsDigit))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "NationalCode must be 10 digits."));
        }

        public override async Task<PersonResponse> CreatePerson(CreatePersonRequest request, ServerCallContext context)
        {
            ValidatePersonFields(request.FirstName, request.LastName, request.NationalCode);

            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                NationalCode = request.NationalCode,
                BirthDate = DateTime.TryParse(request.BirthDate, out var bd) ? bd : DateTime.MinValue
            };

            _db.Persons.Add(person);
            await _db.SaveChangesAsync(context.CancellationToken);

            return new PersonResponse { Person = ToPersonModel(person) };
        }

        public override async Task<PersonResponse> GetPerson(GetPersonRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid id"));

            var person = await _db.Persons.FindAsync(new object[] { id }, context.CancellationToken);
            if (person == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Person not found"));

            return new PersonResponse { Person = ToPersonModel(person) };
        }

        public override async Task<PersonResponse> UpdatePerson(UpdatePersonRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid id"));

            ValidatePersonFields(request.FirstName, request.LastName, request.NationalCode);

            var person = await _db.Persons.FindAsync(new object[] { id }, context.CancellationToken);
            if (person == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Person not found"));

            person.FirstName = request.FirstName;
            person.LastName = request.LastName;
            person.NationalCode = request.NationalCode;
            person.BirthDate = DateTime.TryParse(request.BirthDate, out var bd) ? bd : person.BirthDate;

            _db.Persons.Update(person);
            await _db.SaveChangesAsync(context.CancellationToken);

            return new PersonResponse { Person = ToPersonModel(person) };
        }

        public override async Task<DeletePersonResponse> DeletePerson(DeletePersonRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid id"));

            var person = await _db.Persons.FindAsync(new object[] { id }, context.CancellationToken);
            if (person == null)
                return new DeletePersonResponse { Success = false };

            _db.Persons.Remove(person);
            await _db.SaveChangesAsync(context.CancellationToken);

            return new DeletePersonResponse { Success = true };
        }

        public override async Task<PersonListResponse> GetAllPersons(Empty request, ServerCallContext context)
        {
            var persons = await _db.Persons.ToListAsync(context.CancellationToken);
            var response = new PersonListResponse();
            response.Persons.AddRange(persons.Select(ToPersonModel));
            return response;
        }

        private PersonModel ToPersonModel(Person p)
        {
            return new PersonModel
            {
                Id = p.Id.ToString(),
                FirstName = p.FirstName ?? string.Empty,
                LastName = p.LastName ?? string.Empty,
                NationalCode = p.NationalCode ?? string.Empty,
                BirthDate = p.BirthDate.ToString("o")
            };
        }
    }
}
