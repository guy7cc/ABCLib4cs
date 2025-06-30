namespace ABCLib4cs
{
    public class Class1
    {
        public void DisplayPersonInfo()
        {
            var person = new Models.Person("Alice", 30);
            System.Console.WriteLine(person.ToString());
        }
        public void DisplayJobInfo()
        {
            var job = new Models.Job("Software Engineer", 80000);
            System.Console.WriteLine(job.ToString());
        }

        public static event System.Action<string> OnMessage;

        public class NestedClass
        {
            public const string NestedConstant = "Nested Constant Value";

            public static void DisplayNestedInfo()
            {
                System.Console.WriteLine("This is a nested class.");
            }

            public class InnerNestedClass
            {
                public static void DisplayInnerNestedInfo()
                {
                    System.Console.WriteLine("This is an inner nested class.");
                }
            }
        }
    }

    namespace Models
    {
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Person(string name, int age)
            {
                Name = name;
                Age = age;
            }
            public override string ToString()
            {
                return $"{Name}, {Age} years old";
            }
        }

        public class Job
        {
            public string Title { get; set; }
            public double Salary { get; set; }
            public Job(string title, double salary)
            {
                Title = title;
                Salary = salary;
            }
            public override string ToString()
            {
                return $"{Title} with a salary of {Salary:C}";
            }
        }
    }
}
