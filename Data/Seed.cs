using Dealership.Api.Domain;              //Add intial data to the db. 

namespace Dealership.Api.Data;

public static class Seed
{
    public static async Task RunAsync(AppDbContext db)
    {
        // seed admin (Temp password; real hashing will be added in Phase 2)
        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Email = "admin@local.test1",
                PasswordHash = "TEMP_HASH_TO_BE_REPLACED",
                Role = Role.Admin
            });
        }

        if (!db.Vehicles.Any())
        {
            var cars = new List<Vehicle>
            {
                new() { Vin="VIN10001", Make="Toyota", Model="Camry", Year=2022, Price=92000, Mileage=15000, BodyType="Sedan", Color="White" },
                new() { Vin="VIN10002", Make="Ford", Model="Mustang", Year=2021, Price=185000, Mileage=8000, BodyType="Coupe", Color="Red" },
                new() { Vin="VIN10003", Make="Hyundai", Model="Sonata", Year=2020, Price=78000, Mileage=25000, BodyType="Sedan", Color="Silver" },
                new() { Vin="VIN10004", Make="Kia", Model="Sportage", Year=2023, Price=115000, Mileage=5000, BodyType="SUV", Color="Blue" },
                new() { Vin="VIN10005", Make="Honda", Model="Accord", Year=2019, Price=73000, Mileage=30000, BodyType="Sedan", Color="Black" },
                new() { Vin="VIN10006", Make="Nissan", Model="Altima", Year=2021, Price=76000, Mileage=22000, BodyType="Sedan", Color="Grey" },
                new() { Vin="VIN10007", Make="Chevrolet", Model="Tahoe", Year=2022, Price=235000, Mileage=12000, BodyType="SUV", Color="White" },
                new() { Vin="VIN10008", Make="BMW", Model="330i", Year=2020, Price=165000, Mileage=27000, BodyType="Sedan", Color="Blue" },
                new() { Vin="VIN10009", Make="Mercedes", Model="C200", Year=2019, Price=155000, Mileage=35000, BodyType="Sedan", Color="Black" },
                new() { Vin="VIN10010", Make="Tesla", Model="Model 3", Year=2023, Price=190000, Mileage=6000, BodyType="Sedan", Color="White" }
            };
            db.Vehicles.AddRange(cars);
        }

        await db.SaveChangesAsync();
    }
}
