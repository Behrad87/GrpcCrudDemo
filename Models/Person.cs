namespace GrpcCrudDemo.Models;


public class Person
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string NationalCode { get; set; }

    public DateTime BirthDate { get; set; }
}