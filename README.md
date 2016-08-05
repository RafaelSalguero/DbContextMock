# DbContextMock
##In memory mock EntityFramework 6 DbContext!

Populate your fake database:

```c#
static Guid id = Guid.NewGuid();
...
using (var C = DbContextMock.Persistent<Db>(id))
{
    C.Customer.Add(new Customer
    {
        Name = "Rafa",
        Id = 5,
        Email = "rafaelsalgueroiturrios@gmail.comn"
    });
    
    C.SaveChanges();
}
```

Query your fake database, full support for async queries too!

```c#
using (var C = DbContextMock.Persistent<Db>(id))
{
    return await C.Where(x => x.Id == 5).Select(x => x.Name).ToListAsync();
}
```
