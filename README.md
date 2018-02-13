# GraphQlParser
.Net GraphQl object parser

```
var subSelection = new[]
{
    new GraphQlSubSelection { Name = "id" },
    new GraphQlSubSelection { Name = "name" }
};
var query = GraphQlObjectParser.Parse("query", "users", subSelection);
```

"query{users{id,name}}"


```
var subSelection = new[]
{
    new GraphQlSubSelection { Name = "id" },
    new GraphQlSubSelection { Name = "name" }
};
var query = GraphQlObjectParser.Parse("query", "user", subSelection, new User { Id = 1 });
```

"query{user(id:1){id,name}}"


```
var subSelection = new[]
{
    new GraphQlSubSelection { Name = "id" },
    new GraphQlSubSelection { Name = "name" }
};
var query = GraphQlObjectParser.Parse("mutation", "user", subSelection, new User { Id = 1, Name = "John" }, "data");
```

"mutation{user(data:{id:1,name:"John"}){id,name}}"


```
var subSelection = new[]
{
    new GraphQlSubSelection { Name = "id" },
    new GraphQlSubSelection { Name = "name" },
    new GraphQlSubSelection { Name = "data", SubSelections = new [] { new GraphQlSubSelection { Name = "name" }, new GraphQlSubSelection { Name = "value" }  } }
};
var query = GraphQlObjectParser.Parse("mutation", "user", subSelection, new User { Id = 1, Name = "John" }, "data");
```

"mutation{user(data:{id:1,name:"John"}){id,name,data{name,value}}}"
