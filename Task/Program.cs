using System;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Task
{
    class Program
    {
        static void Main(string[] args)
        {
            ShoppingCart shoppingCart = new ShoppingCart();

            credentials:
            Console.WriteLine("Do you have credentials to Login? (Yes/No): ");
            string credentials = Console.ReadLine();

            if (credentials.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                login:
                Console.Write("Enter Username: ");
                string username = Console.ReadLine();
                Console.Write("Enter Password: ");
                string password = Console.ReadLine();

                bool isValidUser = shoppingCart.ValidateUser(username, password);
                if (isValidUser)
                {
                    Console.WriteLine("Login Successful");
                    shoppingCart.DisplayAndSelectProducts(username);
                }
                else
                {
                    Console.WriteLine("Please enter valid credentials.");
                    goto login;
                }
            }
            else if (credentials.Equals("No", StringComparison.OrdinalIgnoreCase))
            {
                register:
                Console.Write("Enter Name: ");
                string name = Console.ReadLine();
                Console.Write("Enter Username: ");
                string username = Console.ReadLine();
                Console.Write("Enter Password: ");
                string password = Console.ReadLine();
                Console.Write("Confirm Password: ");
                string confirmPassword = Console.ReadLine();
                Console.Write("Enter Mobile Number: ");
                string mobileNumber = Console.ReadLine();

                if (password == confirmPassword)
                {
                    bool isRegistered = shoppingCart.RegisterUser(name, username, password, mobileNumber);
                    if (isRegistered)
                    {
                        Console.WriteLine("Registration Successful");
                        shoppingCart.DisplayAndSelectProducts(username);
                    }
                    else
                    {
                        Console.WriteLine("Username already exists.");
                    }
                }
                else
                {
                    Console.WriteLine("Passwords do not match. Try again");
                    goto register;
                }
            }
            else
            {
                Console.WriteLine("Invalid Input. Please Enter Yes/No.");
                goto credentials;
            }
            Console.Read();
        }
    }

    class ShoppingCart
    {
        private static string connectionString = "server=GSPS8J9S542;integrated security=true;database=pro";

        public bool ValidateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("UserLoginRegisterAndShoppingCart3", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "Login");
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                return dr.HasRows;
            }
        }

        public bool RegisterUser(string name, string username, string password, string mobileNumber)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("UserLoginRegisterAndShoppingCart3", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "Register");
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@MobileNumber", mobileNumber);

                conn.Open();
                int result = (int)cmd.ExecuteScalar();
                return result == 1;
            }
        }

        public void DisplayAndSelectProducts(string username)
        {
            while (true)
            {
                Console.WriteLine("\nAvailable Products:");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("UserLoginRegisterAndShoppingCart3", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetProducts");

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        Console.WriteLine($"ProductID: {dr["ProductID"]}, ProductName: {dr["ProductName"]}, Price: {dr["Price"]}, Quantity: {dr["Qty"]}");
                    }
                }

                Console.Write("Enter ProductID: ");
                int productId = Convert.ToInt32(Console.ReadLine());
                Console.Write("Enter Quantity: ");
                int quantity = Convert.ToInt32(Console.ReadLine());

                bool productSelected = SelectProduct(productId, quantity);
                if (productSelected)
                {
                    Console.WriteLine("Product selected successfully!");
                    AddToCart(username, productId, quantity);
                }
                else
                {
                    Console.WriteLine("Insufficient quantity or invalid ProductID.");
                }

                while (true)
                {
                    Console.Write("Do you want to add more? (Yes/No): ");
                    string addMore = Console.ReadLine();

                    if (addMore.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else if (addMore.Equals("No", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowCartAndCalculateTotal(username);
                        Console.Write("Would you like to place an order? (Yes/No): ");
                        string placeOrder = Console.ReadLine();
                        if (placeOrder.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                        {
                            PlaceOrder(username);
                            Console.WriteLine("Order placed successfully. Thank you for shopping!");
                            return;
                        }
                        else if (placeOrder.Equals("No", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("You have opted not to place an order.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Please enter 'Yes' or 'No'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 'Yes' or 'No'.");
                    }
                }
            }
        }



        private bool SelectProduct(int productId, int quantity)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("UserLoginRegisterAndShoppingCart3", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SelectProduct");
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);

                conn.Open();
                int result = (int)cmd.ExecuteScalar();
                return result == 1;
            }
        }

        private void AddToCart(string username, int productId, int quantity)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("UserLoginRegisterAndShoppingCart3", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "AddToCart");
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void ShowCartAndCalculateTotal(string username)
        {
            decimal totalCost = 0;
            Console.WriteLine("\nYour Cart:");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("UserLoginRegisterAndShoppingCart3", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetCart");
                cmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    string productName = dr["ProductName"].ToString();
                    decimal price = (decimal)dr["Price"];
                    int quantity = (int)dr["Quantity"];
                    Console.WriteLine($"ProductName: {productName}, Quantity: {quantity}, Price: {price}");

                    totalCost += price;
                }
            }

            Console.WriteLine($"\nYour total cost is: {totalCost}");
        }

        private void PlaceOrder(string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("UserLoginRegisterAndShoppingCart3", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "PlaceOrder");
                cmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}