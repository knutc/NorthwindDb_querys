using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EFPrakticum
{
    class Program
    {
        static void Main(string[] args)
        {
            string emptyString = "";
            String antwoord = "0";
            var db = new NorthwindContext();

            while (antwoord != "3")
            {
                var sb = new StringBuilder("Overzicht:" + Environment.NewLine);
                sb.Append("1. Klantbestellingen" + Environment.NewLine);
                sb.Append("2. Data analyse" + Environment.NewLine);
                sb.Append("3. Stop" + Environment.NewLine);
                Console.Write(sb);
                antwoord = Console.ReadLine();

                switch (antwoord)
                {
                    case "1":
                        {
                            CalculateKlantbestellingen();
                            ShowOrderDetail();
                            break;
                        }
                    case "2":
                        {
                            ShowVerkoopPerLanden();
                            break;
                        }
                    default:
                        break;
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nBedankt voor uw medewerking, tot een volgende keer.\nKlik op ENTER om te beëindigen.");
            Console.ReadLine();



            void CalculateKlantbestellingen()
            {
                var orders = from o in db.Orders
                             join c in db.Customers on o.CustomerId equals c.CustomerId
                             join e in db.Employees on o.EmployeeId equals e.EmployeeId
                             select new { OrderNr = o.OrderId, KlantId = c.CustomerId, KlantNaam = c.CompanyName, WerknemerNaam = e.FirstName };

                Console.WriteLine("Order ID".PadRight(10) +
                                  "Klant ID".PadRight(10) +
                                  "Klant naam".PadRight(40) +
                                  "Werknemer naam".PadRight(14));
                Console.WriteLine(emptyString.PadRight(74, '-'));
                foreach (var ord in orders)
                {
                    Console.WriteLine(ord.OrderNr.ToString().PadRight(10) +
                                      ord.KlantId.ToString().PadRight(10) +
                                      ord.KlantNaam.PadRight(40) +
                                      ord.WerknemerNaam.PadRight(14));
                }
                Console.WriteLine();
            }

            void ShowOrderDetail()
            {
                try
                {
                    Console.Write("Geef een order id in om detail info te bekijken: ");
                    int oderid = int.Parse(Console.ReadLine());
                    var orderdetails = from o in db.Orders
                                       join d in db.OrderDetails on o.OrderId equals d.OrderId
                                       join p in db.Products on d.ProductId equals p.ProductId
                                       where o.OrderId == oderid
                                       select new { ProductNr = p.ProductId, ProductNaam = p.ProductName, Aantal = d.Quantity, Eenheidsprijs = d.UnitPrice };

                    Console.WriteLine("Product ID".PadRight(12) +
                                      "Productnaam".PadRight(40) +
                                      "Aantal".PadRight(10) +
                                      "Eenheidsprijs".PadRight(15) +
                                      "Lijnwaarde".PadRight(10));
                    Console.WriteLine(emptyString.PadRight(87, '-'));
                    foreach (var detail in orderdetails)
                    {
                        var product = detail.Eenheidsprijs * detail.Aantal;
                        Console.WriteLine(detail.ProductNr.ToString().PadRight(12) +
                                          detail.ProductNaam.PadRight(40) +
                                          detail.Aantal.ToString().PadRight(10) +
                                          detail.Eenheidsprijs.ToString().PadRight(15) +
                                          product.ToString().PadRight(10));
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Je gaf geen geldig order id, probeer later opnieuw");
                }
                Console.WriteLine();
            }

            void ShowVerkoopPerLanden()
            {
                var result = from c in db.Customers
                             from o in db.Orders
                             from d in db.OrderDetails
                             where c.CustomerId == o.CustomerId
                             where o.OrderId == d.OrderId
                             group d by new { c.Country } into VerkoopPerLand
                             orderby VerkoopPerLand.Key.Country
                             select new
                             {
                                 Land = VerkoopPerLand.Key.Country,
                                 Hoeveelheid = VerkoopPerLand.Sum(d => (d.Quantity * d.UnitPrice))
                             };

                if (result.Count() != 0)
                {
                    Console.WriteLine("Land".PadRight(20) + "Totaal verkocht");
                    Console.WriteLine(emptyString.PadRight(55, '-'));

                    foreach (var dd in result)
                    {
                        Console.WriteLine(dd.Land.PadRight(20) + dd.Hoeveelheid);
                    }
                    ShowLandDetails();
                }
                else
                {
                    Console.WriteLine("De zoekactie gaf geef resultaat terug.");
                }
                Console.WriteLine();
            }

            void ShowLandDetails()
            {
                Console.Write("Geef de landsnaam in om details te bekijken: ");
                var land = Console.ReadLine();
                var result = from c in db.Customers
                             join o in db.Orders on c.CustomerId equals o.CustomerId
                             join d in db.OrderDetails on o.OrderId equals d.OrderId
                             where c.Country == land
                             group d by new { c.CustomerId, c.CompanyName } into LandDetails
                             select new { KlantID = LandDetails.Key.CustomerId, Klantnaam = LandDetails.Key.CompanyName, Totaal = LandDetails.Sum(d => (d.Quantity * d.UnitPrice))};
                if (result.Count() != 0)
                {
                    Console.WriteLine("KlantId".PadRight(10) + "Klantnaam".PadRight(30) + "Totaal");
                    Console.WriteLine(emptyString.PadRight(74, '-'));
                    foreach (var dd in result)
                    {
                        Console.WriteLine(dd.KlantID.PadRight(10) + dd.Klantnaam.PadRight(30) + dd.Totaal);
                    }
                    ShowKlantIdDetails();

                }
                else
                {
                    Console.WriteLine("De zoekactie gaf geef resultaat terug.");
                }
                Console.WriteLine();
            }

            void ShowKlantIdDetails()
            {
                Console.Write("Geef de landsnaam in om details te bekijken: ");
                var klant = Console.ReadLine();
                var result = from o in db.Orders
                             join d in db.OrderDetails on o.OrderId equals d.OrderId
                             where o.CustomerId == klant
                             group d by new
                             {
                                 o.OrderId,
                                 o.OrderDate,
                                 o.ShippedDate
                             } into KlantDetails
                             select new
                             {
                                 OrderID = KlantDetails.Key.OrderId,
                                 OrderDatum = KlantDetails.Key.OrderDate,
                                 Verzenddatum = KlantDetails.Key.ShippedDate,
                                 Totaal = KlantDetails.Sum(d => d.Quantity * d.UnitPrice)
                             };

                if (result.Count() != 0)
                {
                    Console.WriteLine("Order ID".PadRight(10) + "Order datum".PadRight(20) + "Verzenddatum".PadRight(20) + "Totaal");
                    foreach (var dd in result)
                    {
                        Console.WriteLine(dd.OrderID.ToString().PadRight(10) + 
                                          //dd.OrderDatum.ToString().Substring(0, dd.OrderDatum.ToString().IndexOf(" ")).PadRight(15) +
                                          //dd.Verzenddatum.ToString().Substring(0, dd.Verzenddatum.ToString().IndexOf(" ")).PadRight(15) +
                                          dd.OrderDatum.ToString().PadRight(20) + 
                                          dd.Verzenddatum.ToString().PadRight(20) + 
                                          dd.Totaal);
                    }
                }
                else
                {
                    Console.WriteLine("De zoekactie gaf geef resultaat terug.");
                }
                Console.WriteLine();
            }
        }
    }
}
