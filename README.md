# The ORM that understands SQL

Rezoom.SQL is an F# ORM for SQL databases.

It integrates with the F# compiler via a type provider to statically typecheck its own dialect of SQL.
It knows how to translate this SQL dialect to various backends (SQLite, )

This means that it can infer your database model from your migration scripts
(CREATE TABLE, CREATE VIEW, ALTER TABLE, etc.). Then, based on that model, it can validate
all your SQL queries and tell what parameters they take and what types they'll output.

## How about a real example?

Let's say you have a migration script like this:

```sql
-- filename: V1.model.sql
create table Users -- we've got some users
	( Id int primary key autoincrement
	, Name string(64)
	, Email string(128)
	);
create table Groups -- we've got some groups
	( Id int primary key autoincrement
	, Name string(64)
	);
create table UserGroupMaps -- we've got a mapping table that associates users with groups
	( UserId int
	, GroupId int
	, primary key(UserId, GroupId)
	);
```

Now you can write this code in your F# program:

```fsharp
open Rezoom.SQL.Provider
open Rezoom.SQL.Synchronous

type MyQuery = SQL<"""
	select u.Id as UserId, u.Name as UserName, u.Email, g.Id as GroupId, g.Name as GroupName
	from Users u
	join UserGroupMaps m on m.UserId = u.Id
	join Groups g on g.Id = m.GroupId
	where u.Name like '%' || @search || '%'
""">
```

If you flub a name in this query -- say you wrote `Usres` instead of `Users`, you'll get an
error when you try to compile your program. You'll also get an error if you do something that
doesn't make sense from a type standpoint, like writing `sqrt(u.Name)`.

What can you do with the `MyQuery` type?
Rezoom.SQL knows that the command needs a string parameter (for `@search`), and it knows the column names
and types that the query will output. So you can use it without any of the effort of manually creating
`SqlParameter`s and processing `SqlDataReader`s.

```fsharp
[<EntryPoint>]
let main argv =
	use conn = new SqlConnection(connectionString)
	let results = MyQuery.Command(search = "example").Execute(conn)
	for result in results do
		let userId = result.UserId // statically typed as int
		let userName = result.UserName // statically typed as string
		let groupName = result.GroupName // statically typed as string
		printfn "%d %s %s" userId userName groupName
```