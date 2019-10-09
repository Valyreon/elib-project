# elib-project

---

**Database included in the repository needs to be empty. Copy the database to another place, setup the properties file like below and use that database while developing.**

### Making *properties.json* file for database path

To avoid having to change the path to database every time someone changes a path in the code, the database path is going to be
read from **properties.json** file which everyone needs to create for themselves.

Right click on the project that will be executing, go `Add Item` and add `properties.json` file.

Current format of the *properties.json*:
~~~
{
  "DatabasePath": "F:\\\\Path\\\\To\\\\Database\\\\elib_database.sqlite"
}
~~~

Then right click on `properties.json` from Solution Explorer and choose "Properties".
From there, set the `Build Action` to `content` and `Copy to Output Directory` to  `Copy if newer`.

For now you will probably need to do this for *ElibWpf* project and *DatabaseTests* project separately.

To get the Database path in code you use `ApplicationSettings.GetInstance().DatabasePath`. This class will automatically look for *properties.json* file in the current execution folder (where .exe file which is currently executing is) and deserialize it.

---