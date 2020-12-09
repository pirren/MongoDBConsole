using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDbApp
{
    class Program
    {
        private static MongoCRUD ctx;
        static void Main(string[] args)
        {
            ctx = new MongoCRUD("Hotel"); 

            var record = GetById("Invoices", "d6de95e9-23ac-471c-8509-700322cf7766");
            record.Booking.Created = DateTime.Now;
            ctx.UpsertRecords("Invoices", record.Id, record);
            ctx.DeleteRecord<InvoiceModel>("Invoices", record.Id);

            Console.ReadLine();
        }

        private static List<InvoiceModel> GetAll(string table) => ctx.LoadRecords<InvoiceModel>(table);
        private static InvoiceModel GetById(string table, string guid) => ctx.LoadRecordById<InvoiceModel>("Invoices", new Guid(guid));

        static void InsertInvoice(MongoCRUD ctx)
        {
            var invoice = new InvoiceModel
            {
                IsCanceled = false,
                Total = 4800,
                Booking = new BookingModel
                {
                    IsCanceled = false,
                    FirstName = "Kaj",
                    LastName = "Svensson",
                    Email = "kp@mail.com",
                    //BookingNumber = ""
                }
            };
            ctx.InsertRecord("Invoices", invoice);
        }
    }

    public class InvoiceModel
    {
        [BsonId]
        public Guid Id { get; set; }
        public bool IsCanceled { get; set; }
        public int Total { get; set; }
        public BookingModel Booking { get; set; }
    }

    public class BookingModel
    {
        public bool IsCanceled { get; set; }
        [BsonElement("TimeCreated")]
        public DateTime Created { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string BookingNumber { get; set; }
    }

    public class MongoCRUD
    {
        private IMongoDatabase db;

        public MongoCRUD(string database)
        {
            var client = new MongoClient();
            db = client.GetDatabase(database);
        }

        public void InsertRecord<T>(string table, T record)
        {
            var collection = db.GetCollection<T>(table);
            collection.InsertOne(record);
        }

        public List<T> LoadRecords<T>(string table)
        {
            var collection = db.GetCollection<T>(table);

            return collection.Find(new BsonDocument()).ToList();
        }

        public T LoadRecordById<T>(string table, Guid id)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return collection.Find(filter).First();
        }

        public void UpsertRecords<T>(string table, Guid id, T record) 
        {
            var collection = db.GetCollection<T>(table);

            var result = collection.ReplaceOne(
                new BsonDocument("_id", id),
                record,
                new ReplaceOptions { IsUpsert = true });
        }

        public void DeleteRecord<T>(string table, Guid id)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);
            collection.DeleteOne(filter);
        }
    }
}
