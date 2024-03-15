using System.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DadataApi
{
    public class List
    {
        public List<Person> People { get; set; }

        public List()
        {
            People = new List<Person>();
        }

        public void AddPerson(Person person)
        {
            People.Add(person);
        }

        public void Display()
        {
            foreach (var person in People)
            {
                Console.WriteLine(person);
                Console.WriteLine();
            }
        }

        public void saveFile()
        {
            using (StreamWriter writer = new StreamWriter("DadataPeopleList.txt", append: true))
            {
                foreach (var person in People)
                {
                    writer.WriteLine(person);
                    writer.WriteLine();
                }
            }
        }
    }

    public class Person
    {
        public string PhoneNumber { get; set; }
        public string Operator { get; set; }
        public string Initials { get; set; }
        public string INN { get; set; }
        public string Country { get; set; }
        public string Gender { get; set; }

        public override string ToString()
        {
            return $" {Initials}:\n Номер телефона: {PhoneNumber}\n Мобильный оператор: {Operator}\n ИНН: {INN}\n Страна: {Country}\n Пол: {Gender}";
        }

        public void saveFile()
        {
            using (StreamWriter writer = new StreamWriter("zxc.txt", append: true))
            {
                writer.WriteLine(this);
                writer.WriteLine();
            }
        }
    }

    public class Request
    {
        public string Query { get; set; }
    }

    public class PrettyName
    {
        public string Result { get; set; }
        public string Gender { get; set; }
    }

    public class NumberInfo
    {
        public string Phone { get; set; }
        public string Provider { get; set; }
        public string Country { get; set; }
    }

    public class InnInfo
    {
        public List<Suggestions> Suggestions { get; set; }
    }

    public class Suggestions
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public Management Management { get; set; }
    }

    public class Management
    {
        public string Name { get; set; }
    }

    public class Program
    {
        static HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Token c78183004cdf0f8819976ed80d36b9980329b624");
                httpClient.DefaultRequestHeaders.Add("X-Secret", "dbd66ffae7be75984586c0a20f4196c3ab088332");

                List peopleList = new List();

                while (true)
                {
                    Console.WriteLine("Информации о человеке");
                    Console.WriteLine("1)добавить человека");
                    Console.WriteLine("2)отобразить данные о человеке");
                    Console.WriteLine("3)сохранить в файл");
                    Console.Write("Ваш выбор: ");
                    int choice = Convert.ToInt32(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            DadataApi.Person personAdd = new DadataApi.Person();

                            Console.WriteLine("Введите номер телефона:");
                            string phoneNumber = Console.ReadLine();
                            var numberResponse = httpClient.PostAsJsonAsync("https://cleaner.dadata.ru/api/v1/clean/phone", new[] { phoneNumber }).Result;

                            var numberResult = numberResponse.Content.ReadFromJsonAsync<List<DadataApi.NumberInfo>>().Result;

                            foreach (var numberInfo in numberResult)
                            {
                                personAdd.PhoneNumber = numberInfo.Phone;
                                personAdd.Country = numberInfo.Country;
                                personAdd.Operator = numberInfo.Provider;
                            }

                            Console.WriteLine("Введите ИНН:");
                            string inn = Console.ReadLine();
                            var innResponse = httpClient.PostAsJsonAsync("http://suggestions.dadata.ru/suggestions/api/4_1/rs/findById/party", new DadataApi.Request { Query = inn }).Result;
                            var innInfo = innResponse.Content.ReadFromJsonAsync<DadataApi.InnInfo>().Result;

                            personAdd.INN = inn;
                            personAdd.Initials = innInfo.Suggestions[0].Data.Management.Name;

                            string[] names = { personAdd.Initials };
                            var nameResponse = httpClient.PostAsJsonAsync("https://cleaner.dadata.ru/api/v1/clean/name", names).Result;
                            var prettyNames = nameResponse.Content.ReadFromJsonAsync<List<DadataApi.PrettyName>>().Result;

                            foreach (var prettyName in prettyNames)
                            {
                                personAdd.Initials = prettyName.Result;
                                personAdd.Gender = prettyName.Gender;
                            }

                            peopleList.AddPerson(personAdd);
                            break;


                        case 2:
                            peopleList.Display();
                            break;

                        case 3:
                            peopleList.saveFile();
                            Console.WriteLine("Список людей сохранен в txt файл.");
                            return;

                        default:
                            Console.WriteLine("Неверный выбор.Попробуйте ещё раз.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}