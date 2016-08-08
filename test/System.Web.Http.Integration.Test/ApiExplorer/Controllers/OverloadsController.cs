// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.ApiExplorer
{
    public class OverloadsController : ApiController
    {
        public Person Get(int id) { return null; }
        public List<Person> Get() { return null; }
        public List<Person> Get(string name) { return null; }
        public Person GetPersonByNameAndId(string name, int id) { return null; }
        public Person GetPersonByNameAndAge(string name, int age) { return null; }
        public Person GetPersonByNameAgeAndSsn(string name, int age, int ssn) { return null; }
        public Person GetPersonByNameIdAndSsn(string name, int id, int ssn) { return null; }
        public Person GetPersonByNameAndSsn(string name, int ssn) { return null; }
        public Person Post(Person Person) { return null; }
        public Person ActionDefaultedToPost(string name, int age) { return null; }
        public void Delete(int id, string name = "Default Name") { }
        public void Delete(int id, string name, int age) { }

        public class Person
        {
            public string Name { get; set; }
            public int ID { get; set; }
            public int SSN { get; set; }
            public int Age { get; set; }
        }
    }
}
