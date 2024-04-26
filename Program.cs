
using Microsoft.EntityFrameworkCore;
using PersonInterestsApi.Data;
using PersonInterestsApi.Models;
using System;
using System.Linq;

namespace PersonInterestsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //--------------------------- PERSON ------------------------------------------------------------

            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------
            // LABB UPPGIFT 1 Hämta alla personer i systemet
            //  GET ALL PERSONS
            app.MapGet("/persons", async (AppDbContext context) =>
            {
                var persons = await context.Persons.ToListAsync();
                if (!persons.Any())
                {
                    return Results.NotFound("No Person's found");
                }

                return Results.Ok(persons);
            });
            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------

            // GET ONE PERSON
            app.MapGet("/persons/{id:int}", async (int id, AppDbContext context) =>
            {
                var person = await context.Persons.FindAsync(id);
                if (person == null)
                {
                    return Results.NotFound($"No Person with id: {id} found");
                }

                return Results.Ok(person);
            });

            // POST A NEW PERSON
            app.MapPost("/persons", async (Person person, AppDbContext context) =>
            {
                context.Persons.Add(person);
                await context.SaveChangesAsync();
                return Results.Created($"/persons/{person.PersonId}", person);
            });

            // PUT A EXCISTING PERSON
            app.MapPut("/persons/{id:int}", async (int id, Person updatedPerson, AppDbContext context) =>
            {
                var person = await context.Persons.FindAsync(id);
                if(person == null)
                {
                    return Results.NotFound($"Could not find person with id: {id}");
                }
                person.PersonName = updatedPerson.PersonName;
                person.PersonPhone = updatedPerson.PersonPhone;
                context.Persons.Update(person);
                await context.SaveChangesAsync();
                return Results.Ok(person);

            });

            //--------------------------- INTEREST ------------------------------------------------------------
            // GET ALL INTERESTS
            app.MapGet("/interests", async (AppDbContext context) =>
            {
                var interests = await context.Interests.ToListAsync();
                if (!interests.Any())
                {
                    return Results.NotFound("No Interest's found");
                }

                return Results.Ok(interests);
            });
            // POST A NEW INTEREST
            //---------------------------------------------------------------------------------------
            //LABB UPPGIFT Koppla en person till ett nytt intresse
            //POST PersonInterest
            //---------------------------------------------------------------------------------------
            app.MapPost("/interests", async (Interest interest, AppDbContext context) =>
            {
                context.Interests.Add(interest);
                await context.SaveChangesAsync();
                return Results.Created($"/persons/{interest.InterestId}", interest);
            });

            // PUT A EXCISTING INTEREST
            app.MapPut("/interests/{id:int}", async (int id, Interest updatedInterest, AppDbContext context) =>
            {
                var interest = await context.Interests.FindAsync(id);
                if (interest == null)
                {
                    return Results.NotFound($"Could not find person with id: {id}");
                }

                interest.InterestTitle = updatedInterest.InterestTitle;
                interest.InterestDescription = updatedInterest.InterestDescription;
                context.Interests.Update(interest);
                await context.SaveChangesAsync();
                return Results.Ok(interest);

            });
            //--------------------------- PERSON-INTEREST ------------------------------------------------------------

            app.MapGet("/personinterests", async (AppDbContext context) =>
            {
                var personInterests = await context.PersonInterests.ToListAsync();

                if (!personInterests.Any())
                {
                    return Results.NotFound("No PersonInterest's found");
                }

                return Results.Ok(personInterests);
            });

            //---------------------------------------------------------------------------------------
            //LABB UPPGIFT Koppla en person till ett nytt intresse
            //POST PersonInterest
            //---------------------------------------------------------------------------------------

            app.MapPost("/personinterests", async (PersonInterest personInterest, AppDbContext context) =>
            {
                context.PersonInterests.Add(personInterest);
                await context.SaveChangesAsync();
                return Results.Created($"/persons/{personInterest.PersonInterestId}", personInterest);
            });

            //--------------------------- LINK ------------------------------------------------------------

            //Hämtar alla links objekt som finns
            app.MapGet("/links", async (AppDbContext context) =>
            {
                var link = await context.Links.ToListAsync();
                if (!link.Any())
                {
                    return Results.NotFound("NO link's found");
                }

                return Results.Ok(link);
            });

            app.MapPut("/links/edit/{id:int}", async (int id, Link updatedLink, AppDbContext context) =>
            {
                var link = await context.Links.FindAsync(id);
                if (link == null)
                {
                    return Results.NotFound($"Could not find linkID: {id}");
                }
                link.LinkAddress = updatedLink.LinkAddress;

                return Results.Accepted("Updated link address! ", link);
            });

            //Här hämtas alla länkar som ligger kopplad till alla intressen som finns

            app.MapGet("/links/on/interests", async (AppDbContext context) =>
            {
                var result = from pi in context.PersonInterests
                             join i in context.Interests on pi.FkInterestId equals i.InterestId
                             join l in context.Links on i.InterestId equals l.FkInterestId
                             select new
                             {
                                 i.InterestTitle,
                                 l.LinkAddress
                             };
                var groupedResult = result.GroupBy(r => r.InterestTitle)
                              .Select(grp => new
                              {
                                  InterestTitle = grp.Key,
                                  LinkAddresses = grp.Select(r => r.LinkAddress).ToList()
                              });

                return Results.Ok(groupedResult);
            });

            app.MapPost("/links", async (Link link, AppDbContext context) =>
            {
                context.Links.Add(link);
                await context.SaveChangesAsync();
                return Results.Created($"/link/{link.LinkId}", link);
            });


            //---------------------------------------------------------------------------------------
            //LABB UPPGIFT Lägga in nya länkar för en specifik person och ett specifikt intresse
            //POST PersonInterest
            //---------------------------------------------------------------------------------------
            // HÄR får man skapa en ny länk på en redan existeraden person och ett befintligt intresse
            app.MapPost("/persons/{personId}/interests/{interestId}/links", async (AddLinkOnExistingPersonModel model, AppDbContext context) =>
            {
                try
                {
                    var person = await context.Persons.FindAsync(model.PersonId);
                    var interest = await context.Interests.FindAsync(model.InterestId);
                    if (person == null || interest == null)
                    {
                        return Results.NotFound();
                    }

                    var link = new Link
                    {
                        FkPersonId = model.PersonId,
                        FkInterestId = model.InterestId,
                        LinkAddress = model.LinkAddress
                    };

                    context.Links.Add(link);
                    await context.SaveChangesAsync();

                    return Results.Created($"/person/{model.PersonId}/interest/{model.InterestId}/link/{link.LinkId}", link);
                }
                catch (Exception)
                {
                    return Results.Problem();
                }
            });




            //--------------------------- PERSON and INTEREST ------------------------------------------------------------

            //GET all persons with there interests
            app.MapGet("/persons/interests", async (AppDbContext context) =>
            {
                var result = from p in context.Persons
                             join pi in context.PersonInterests on p.PersonId equals pi.FkPersonId
                             join i in context.Interests on pi.FkInterestId equals i.InterestId
                             select new
                             {
                                 p.PersonId,
                                 p.PersonName,
                                 p.PersonPhone,
                                 i.InterestTitle,
                             };
                var groupedResult = result.GroupBy(x => x.PersonId)
                    .Select(grp => new
                    {
                        PersonId = grp.Key,
                        PersonName = grp.First().PersonName,
                        PersonPhone = grp.First().PersonPhone,
                        InterestTitle = grp.Select(i => i.InterestTitle).ToList(),
                    });

                return Results.Ok(groupedResult);
            });


            //---------------------------------------------------------------------------------------
            //LABB UPPGIFT Hämta alla intressen som är kopplade till en specifik person
            //GET one person with interests
            //---------------------------------------------------------------------------------------
            app.MapGet("/persons/interests/{id:int}", async (int id, AppDbContext context) =>
            {

                var person = await context.Persons.FindAsync(id);
                if (person == null)
                {
                    return Results.NotFound($"Could not find person with id: {id}");
                }
                else
                {
                    var result = from p in context.Persons.Where(x => x.PersonId == id)
                                 join pi in context.PersonInterests on p.PersonId equals pi.FkPersonId
                                 join i in context.Interests on pi.FkInterestId equals i.InterestId
                                 select new
                                 {
                                     p.PersonId,
                                     p.PersonName,
                                     p.PersonPhone,
                                     i.InterestTitle,
                                 };
                    var groupedResult = result.GroupBy(x => x.PersonId)
                        .Select(grp => new
                        {
                            PersonId = grp.Key,
                            PersonName = grp.First().PersonName,
                            PersonPhone = grp.First().PersonPhone,
                            InterestTitle = grp.Select(i => i.InterestTitle).ToList()
                        });
                    
                    return Results.Ok(groupedResult);
                }
                
            });
            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------




            //---------------------------------------------------------------------------------------
            //LABB EXTRA Här skapar jag en ny person och ett nytt intresse till den personen på samma gång
            //
            //---------------------------------------------------------------------------------------
            app.MapPost("persons/interests", async (PersonWithInterestModel model, AppDbContext context) =>
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    // Lägger till personen
                    context.Persons.Add(model.Person);
                    await context.SaveChangesAsync();

                    // Lägger till intresset
                    context.Interests.Add(model.Interest);
                    await context.SaveChangesAsync();

                    //Och skapar en PersonInterest på en gång
                    var personInterest = new PersonInterest
                    {
                        FkPersonId = model.Person.PersonId,
                        FkInterestId = model.Interest.InterestId
                    };
                    context.PersonInterests.Add(personInterest);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Results.Created($"/person/interest/{personInterest.PersonInterestId}", personInterest);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Results.Problem("A problem when trying to add person and intereset");
                }
            });


            //--------------------------- PERSON, INTEREST, LINKS ------------------------------------------------------------

            //Här får jag ut alla personer och deras intressen med länkar

            app.MapGet("/allpersoninfo", async (AppDbContext context) =>
            {
                var result = from p in context.Persons
                             join pi in context.PersonInterests on p.PersonId equals pi.FkPersonId
                             join i in context.Interests on pi.FkInterestId equals i.InterestId
                             join l in context.Links on i.InterestId equals l.FkInterestId
                             select new
                             {
                                 p.PersonName,
                                 i.InterestTitle,
                                 l.LinkAddress
                             };

                var groupedResult = result.GroupBy(x => x.PersonName)
                          .Select(grp => new
                          {
                              PersonName = grp.Key,
                              Interests = grp.GroupBy(x => x.InterestTitle)
                                             .Select(grp2 => new
                                             {
                                                 InterestTitle = grp2.Key,
                                                 LinkAddresses = grp2.Select(x => x.LinkAddress).ToList()
                                             })
                                             .ToList()
                          });

                return Results.Ok(groupedResult);
            });

            //---------------------------------------------------------------------------------------
            //LABB UPPGIFT Ge möjlighet till den som anropar APIet och efterfrågar en person att direkt få ut alla intressen och alla länkar för den personen i en hierarkisk JSON-fil
            //GET one person with interests and links
            //---------------------------------------------------------------------------------------
            //Här får jag ut alla intressen och länkar för endast 1 person baserat på dennas PersonId
            app.MapGet("/interests/links/for/person/{personId}", async (int id, AppDbContext context) =>
            {
                var result = from p in context.Persons.Where(x => x.PersonId == id)
                             join pi in context.PersonInterests on p.PersonId equals pi.FkPersonId
                             join i in context.Interests on pi.FkInterestId equals i.InterestId
                             join l in context.Links on i.InterestId equals l.FkInterestId
                             select new
                             {
                                 p.PersonName,
                                 i.InterestTitle,
                                 l.LinkAddress
                             };

                var groupedResult = result.GroupBy(x => x.PersonName)
                          .Select(grp => new
                          {
                              PersonName = grp.Key,
                              Interests = grp.GroupBy(x => x.InterestTitle)
                                             .Select(grp2 => new
                                             {
                                                 InterestTitle = grp2.Key,
                                                 Links = grp2.Select(x => x.LinkAddress).ToList()
                                             })
                                             .ToList()
                          })
                          .FirstOrDefault();

                return Results.Ok(groupedResult);
            });

            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------
            //LABB UPPGIFT Hämta alla länkar som är kopplade till en specifik person
            //Här får man mata in ett personID och får då se alla länkar som den personen har lagt in

            app.MapGet("/all/links/added/by/person/{id:int}", async (int id, AppDbContext context) =>
            {
                var person = await context.Persons.FindAsync(id);
                if (person == null)
                {
                    return Results.NotFound($"Could not find person with id: {id}");
                }
                else
                {
                    var result = from p in context.Persons.Where(x => x.PersonId == id)
                                 join l in context.Links on p.PersonId equals l.FkPersonId
                                 select new
                                 {
                                     p.PersonName,
                                     l.LinkAddress
                                 };
                    var groupedResult = result.GroupBy(r => r.PersonName)
                    .Select(grp => new
                    {
                        PersonName = grp.Key,
                        LinkAddress = grp.Select(x => x.LinkAddress)
                    });
                    return Results.Ok(groupedResult);
                }
                
            });
            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------


            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------

            //LABB EXTRA Här får man söka efter ett namn eller bara en bokstav och får då ut alla personer som har det i PersonName sen visas alla deras 
            //intressen och länkar till dem intressena
            app.MapGet("/searchForPerson/{name}/info", async (string name, AppDbContext context) =>
            {
                var result = from p in context.Persons
                             where p.PersonName.Contains(name)
                             join pi in context.PersonInterests on p.PersonId equals pi.FkPersonId
                             join i in context.Interests on pi.FkInterestId equals i.InterestId
                             join l in context.Links on i.InterestId equals l.FkInterestId
                             select new
                             {
                                 p.PersonName,
                                 i.InterestTitle,
                                 l.LinkAddress
                             };

                var groupedResult = result.GroupBy(x => x.PersonName)
                          .Select(grp => new
                          {
                              PersonName = grp.Key,
                              Interests = grp.GroupBy(x => x.InterestTitle)
                                             .Select(grp2 => new
                                             {
                                                 InterestTitle = grp2.Key,
                                                 LinkAddresses = grp2.Select(x => x.LinkAddress).ToList()
                                             })
                                             .ToList()
                          });

                
                return Results.Ok(groupedResult);
            });
            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------



            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------
            //LABB EXTRA, Paginering där man får välja hur många resultat man vill skapa för en sida

            app.MapGet("/persons/paginate", async (AppDbContext context, int pageSize = 10, int pageNumber = 1) =>
            {
                var persons = await context.Persons.ToListAsync();
                if (!persons.Any())
                {
                    return Results.NotFound("No Person's found");
                }

                var totaltResults = persons.Count();

                var pagniantedResult = persons.Skip((pageNumber -1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();


                return Results.Ok(pagniantedResult);
            });
            //---------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------



            //LABB EXTRA, Paginering där man får välja igen men då svaret innehåller fler results för varje person så blir denna lite konstig

            app.MapGet("/allpersoninfo/paginate", async (AppDbContext context, int pageSize = 10, int pageNumber = 1) =>
            {
                var result = from p in context.Persons
                             join pi in context.PersonInterests on p.PersonId equals pi.FkPersonId
                             join i in context.Interests on pi.FkInterestId equals i.InterestId
                             join l in context.Links on i.InterestId equals l.FkInterestId
                             select new
                             {
                                 p.PersonName,
                                 i.InterestTitle,
                                 l.LinkAddress
                             };

                var totalItems = await result.CountAsync();

                var paginatedResult = await result.Skip((pageNumber - 1) * pageSize)
                                                 .Take(pageSize)
                                                 .GroupBy(x => x.PersonName)
                                                 .Select(grp => new
                                                 {
                                                     PersonName = grp.Key,
                                                     Interests = grp.GroupBy(x => x.InterestTitle)
                                                                    .Select(grp2 => new
                                                                    {
                                                                        InterestTitle = grp2.Key,
                                                                        LinkAddresses = grp2.Select(x => x.LinkAddress).ToList()
                                                                    })
                                                                    .ToList()
                                                 })
                                                 .ToListAsync();

                var paginationInfo = new
                {
                    TotalItems = totalItems,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                };

                var response = new
                {
                    Pagination = paginationInfo,
                    Results = paginatedResult
                };

                return Results.Ok(response);
            });

            app.Run();
        }
    }
}
