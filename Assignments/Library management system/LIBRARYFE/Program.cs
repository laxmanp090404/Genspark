using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata;
using LIBRARYBL.Services;
using LIBRARYDALL.Context;
using LIBRARYDALL.Repositories;
using LIBRARYMODELS.Models;

namespace LIBRARYFE
{
    public class Program
    {
        LibraryContext context;
        //services to access
        MemberService memberService;
        BookService bookService;
        BorrowingService borrowingService;
        FineService fineService;
        ReportService reportService;

        //manage app close and menu
        bool exitApp = false;

        public Program()
        {
            context = new LibraryContext();
            //injecting dbcontext for all Repositories
            // these repos are injected again to services
            MemberRepository memberRepository = new MemberRepository(context);
            BookRepository bookRepository = new BookRepository(context);
            BorrowingRepository borrowingRepository = new BorrowingRepository(context);
            FineRepository fineRepository = new FineRepository(context);

            // injecting to services
            memberService = new MemberService(memberRepository);
            bookService = new BookService(bookRepository, context);
            borrowingService = new BorrowingService(borrowingRepository, memberRepository, fineRepository, context);
            fineService = new FineService(fineRepository, context);
            reportService = new ReportService(context);
        }
        static void Main(string[] args)
        {
            Program app = new Program();
            app.RunApp();

        }
        private void RunApp()
        {


            while (!exitApp)
            {
                try
                {
                    Console.WriteLine("\nWelcome to Library Management System");
                    displayMainMenu();

                    int choice = getIntChoice(7, "Please enter your choice of service");
                    switch (choice)
                    {
                        case 1:
                            ManageMembers();
                            break;
                        case 2:
                            ManageBooks();
                            break;
                        case 3:
                            BorrowBook();
                            break;
                        case 4:
                            ReturnBook();
                            break;
                        case 5:
                            ManageFine();
                            break;
                        case 6:
                            ReportsMenu();
                            break;
                        case 7:
                            exitAppHelper();
                            break;
                            //default not required as i handle with getIntChoice func
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occured " + e.Message + " at " + e.Source);
                }


            }

        }
        //reports menu
        private void ReportsMenu()
        {
            while (true)
            {
                Console.WriteLine("\nReports Console\n1.Currently Borrowed Books\n2.Overdue Borrowings\n3.Members With Pending Fines\n4.Most Borrowed Books\n5.Member Borrowing History\n6.Available Books By Category\n7.Back");
                int choice = getIntChoice(7, "Enter your choice");

                switch (choice)
                {
                    case 1:
                        ShowCurrentlyBorrowedBooks();
                        break;
                    case 2:
                        ShowOverdueBorrowings();
                        break;
                    case 3:
                        ShowMembersWithPendingFines();
                        break;
                    case 4:
                        ShowMostBorrowedBooks();
                        break;
                    case 5:
                        ShowBorrowingHistory();
                        break;
                    case 6:
                        ShowAvailableBooksByCategory();
                        break;
                    case 7:
                        return;
                }
            }
        }
        // show available books by category
        private void ShowAvailableBooksByCategory()
        {
            Console.WriteLine("Enter category name : ");
            string category = Console.ReadLine()?.Trim() ?? "";
            ICollection<BookCopy> copies = reportService.GetAvailableBooksByCategory(category);
            PrintBookCopies(copies, "Available Books By Category");
        }
        // show borrowing history for a member
        private void ShowBorrowingHistory()
        {
            Console.WriteLine("Enter member id : ");
            int memberId = getIntInputNoRange();
            ICollection<Borrowing> borrowings = reportService.GetBorrowingHistoryByMember(memberId);
            PrintBorrowings(borrowings, "Borrowing History");
        }
        //show most borrowed books
        private void ShowMostBorrowedBooks()
        {
            ICollection<Book> books = reportService.GetMostBorrowedBooks();
            PrintBooks(books, "Most Borrowed Books");
        }
        // show members with pending fines
        private void ShowMembersWithPendingFines()
        {
            ICollection<Member> members = reportService.GetMembersWithPendingFines();
            PrintMembers(members, "Members With Pending Fines");
        }
        // show Overdue borrowings
        private void ShowOverdueBorrowings()
        {
            ICollection<Borrowing> borrowings = reportService.GetOverdueBorrowings();
            PrintBorrowings(borrowings, "Overdue Borrowings");
        }
        // show currently borrowed books
        private void ShowCurrentlyBorrowedBooks()
        {
            ICollection<Borrowing> borrowings = reportService.GetCurrentlyBorrowedBooks();
            PrintBorrowings(borrowings, "Currently Borrowed Books");
        }
        // book copies print helper
        private void PrintBookCopies(ICollection<BookCopy> copies, string header)
        {
            Console.WriteLine();
            Console.WriteLine(header);

            if (copies.Count == 0)
            {
                Console.WriteLine("No copies found");
                return;
            }

            foreach (var copy in copies)
            {
               Console.WriteLine($"\nCopy Id : {copy.CopyId}\nBook : {copy.Edition?.Book?.Title}\nEdition : {copy.Edition?.EditionNumber}\nStatus : {copy.CopyStatus}");
            }
        }

        //print members helper
        private void PrintMembers(ICollection<Member> members, string header)
        {
            Console.WriteLine();
            Console.WriteLine(header);

            if (members.Count == 0)
            {
                Console.WriteLine("No members found");
                return;
            }

            foreach (var member in members)
            {
                decimal pendingAmount = member.Fines
                    .Where(f => f.FineStatus == "pending")
                    .Sum(f => f.DaysOverdue * f.FinePerDay);

              Console.WriteLine($"\nMember Id : {member.MemberId}\nName : {member.FullName}\nEmail : {member.Email}\nPending Fine : ₹{pendingAmount}");
            }
        }
        // print books helper
        private void PrintBooks(ICollection<Book> books, string header)
        {
            Console.WriteLine();
            Console.WriteLine(header);

            if (books.Count == 0)
            {
                Console.WriteLine("No books found");
                return;
            }

            foreach (var book in books)
            {
                int totalBorrowings = book.BookEditions
                    .SelectMany(e => e.BookCopies)
                    .SelectMany(c => c.Borrowings)
                    .Count();

                Console.WriteLine($"\nBook Id : {book.BookId}\nTitle : {book.Title}\nAuthor : {book.Author}\nTotal Borrowings : {totalBorrowings}");
            }
        }

        //print borrow helper
        private void PrintBorrowings(ICollection<Borrowing> borrowings, string header)
        {
            Console.WriteLine();
            Console.WriteLine(header);

            if (borrowings.Count == 0)
            {
                Console.WriteLine("No records found");

                return;
            }

            foreach (var borrowing in borrowings)
            {
                Console.WriteLine($"\nBorrowing Id : {borrowing.BorrowingId}\nMember : {borrowing.Member?.FullName}\nBook : {borrowing.Copy?.Edition?.Book?.Title}\nBorrowed On : {borrowing.BorrowedOn}\nDue Date : {borrowing.DueDate}\nStatus : {borrowing.BorrowStatus}");
            }
        }
        // fine managing menu
        private void ManageFine()
        {
            while (true)
            {
                Console.WriteLine("\nFine Management Console\n1.View Pending Amount\n2.View Fine History\n3.Pay Fine\n4.Back");

                int choice = getIntChoice(4, "Enter your choice");

                switch (choice)
                {
                    case 1:
                        getPendingfine();
                        break;
                    case 2:
                        ViewFineHistory();
                        break;
                    case 3:
                        payFine();
                        break;
                    case 4:
                        return;
                }
            }
        }
        // helper to view fine history
        private void ViewFineHistory()
        {
            Console.WriteLine("Enter member id : ");

            int memberId = getIntInputNoRange();

            ICollection<Fine> fines =
                fineService.GetMemberFineHistory(memberId);

            if (fines.Count == 0)
            {
                Console.WriteLine("No fine history found");

                return;
            }
            foreach (var fine in fines)
            {
                decimal total = fine.DaysOverdue * fine.FinePerDay;
                Console.WriteLine($"\nFine Id : {fine.FineId}\nBorrowing Id : {fine.BorrowingId}\nDays Overdue : {fine.DaysOverdue}\nFine Amount : ₹{total}\nStatus : {fine.FineStatus}");
                if (fine.FineStatus == "waived")
                {
                    Console.WriteLine($"Waiver Reason : {fine.WaiverReason}");
                }
            }
        }
        // pay fine 
        private void payFine()
        {
            Console.WriteLine("Need to view fines first? Y/N");

            string choice =
                Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (choice == "Y")
            {
                ViewFineHistory();
            }

            Console.WriteLine("Enter fine id : ");

            int fineId = getIntInputNoRange();
            Console.WriteLine("\nPayment Modes\n1.Cash\n2.UPI\n3.Card\n4.Online");
            int paymentChoice = getIntChoice(4, "Select payment mode");
            string paymentMode = "";

            switch (paymentChoice)
            {
                case 1:
                    paymentMode = "cash";
                    break;
                case 2:
                    paymentMode = "upi";
                    break;
                case 3:
                    paymentMode = "card";
                    break;
                case 4:
                    paymentMode = "online";
                    break;
            }

            Console.WriteLine("Enter remarks (optional) : ");

            string remarks =
                Console.ReadLine()?.Trim() ?? "";

            fineService.PayFine(fineId, paymentMode, remarks);
        }
        private void getPendingfine()
        {
            System.Console.WriteLine("Enter member Id : ");
            int memberId = getIntInputNoRange();
            decimal amount = fineService.GetPendingFineAmount(memberId);
            System.Console.WriteLine($"Pending fine :  ₹{amount}");
        }

        // return book
        private void ReturnBook()
        {
            System.Console.WriteLine("Enter your member id to track your borrowings:");
            int memberId = getIntInputNoRange();
            //get all fines for that particular member
            ICollection<Borrowing> borrowings = borrowingService.GetMemberBorrowings(memberId);
            if (borrowings.Count == 0)
            {
                Console.WriteLine("No active borrowings for the given member \nSo you can't return anything");
                return;
            }
            System.Console.WriteLine("Need to view active borrowings? Y/N");
            string choice = Console.ReadLine()?.Trim() ?? "";
            if (choice.ToUpper() == "Y")
            {
                foreach (var borrowing in borrowings)
                {
                    System.Console.WriteLine($"\nBorrowing Id : {borrowing.BorrowingId}");
                    System.Console.WriteLine($"Copy Id : {borrowing.CopyId} : {borrowing.Copy.Edition.Book.Title}");
                    System.Console.WriteLine($"Due date : {borrowing.DueDate}");
                }
            }
            Console.WriteLine("Enter borrowing id :");
            int borrowingId = getIntInputNoRange();
            System.Console.WriteLine("Return typse : ");
            System.Console.WriteLine("1.Normal\n2.Damaged Usable\n3.Damaged Unusable\n4.Lost");

            int returnchoice = getIntChoice(4, "Please enter return type id of your choice");
            string returnType = "";
            switch (returnchoice)
            {
                case 1:
                    returnType = "normal";
                    break;
                case 2:
                    returnType = "damaged_usable";
                    break;
                case 3:
                    returnType = "damaged_unusable";
                    break;
                case 4:
                    returnType = "lost";
                    break;
            }
            string? damagedescription = null;
            if (returnType != "normal")
            {
                System.Console.WriteLine("Enter damage description :");
                damagedescription = Console.ReadLine()?.Trim() ?? "";
            }
            borrowingService.ReturnBook(borrowingId, returnType, damagedescription);

        }

        // borrowbook menu
        private void BorrowBook()
        {
            System.Console.WriteLine("Enter the member id :");
            int memberId = getIntInputNoRange();
            System.Console.WriteLine("Don't know the book copy Id you want? Search for it (Y/N)");
            string choice = Console.ReadLine()?.Trim() ?? "";
            if (choice.ToUpper() == "Y")
            {
                InventoryManagement();
            }
            System.Console.WriteLine("Enter the copy Id : ");
            int copyId = getIntInputNoRange();
            // borrow by member of copy
            borrowingService.BorrowBook(memberId, copyId);
        }
        private int getIntInputNoRange()
        {
            int option;
            while (!int.TryParse(Console.ReadLine(), out option) || option <= 0)
            {
                System.Console.WriteLine("Enter a valid option");
            }
            return option;
        }
        // manage books menu
        private void ManageBooks()
        {
            while (true)
            {
                Console.WriteLine("\nBook Management Console\n1.Browse Catalog\n2.Search Books\n3.Add Book\n4.Add Edition\n5.Add Copies\n6.Inventory Management\n7.Back");

                int choice = getIntChoice(7, "Enter your choice");

                switch (choice)
                {
                    case 1:
                        BrowseCatalog();
                        break;

                    case 2:
                        SearchBooksMenu();
                        break;

                    case 3:
                        AddBook();
                        break;

                    case 4:
                        AddEdition();
                        break;

                    case 5:
                        AddCopies();
                        break;

                    case 6:
                        InventoryManagement();
                        break;

                    case 7:
                        return;
                }
            }
        }

        // method to add copies
        private void AddCopies()
        {
            System.Console.WriteLine("Need to view Editions first? Y/N");
            string choice = Console.ReadLine()?.Trim() ?? "";
            if (choice.ToUpper() == "Y")
            {

                ICollection<BookEdition> editions = bookService.GetEditions();

                if (editions.Count == 0)
                {
                    Console.WriteLine("No editions found");
                }
                else
                {
                    foreach (var edition in editions)
                    {
                        System.Console.WriteLine($"Edition Id :{edition.EditionId} : Title : {edition.Book.Title}  Edition : {edition.EditionNumber} / {edition.EditionLabel}");
                    }

                }
            }
            System.Console.WriteLine("Enter edtion Id :");
            int editionId = getIntInputNoRange();
            System.Console.WriteLine("How many copies to add");
            int count = getIntInputNoRange();
            for (int i = 1; i <= count; i++)
            {
                BookCopy copy = new BookCopy();
                copy.EditionId = editionId;
                copy.CopyStatus = "Available";
                copy = bookService.AddCopy(copy);
                if (copy == null)
                {
                    System.Console.WriteLine($"Failed to add copy {i}");
                }
            }
            System.Console.WriteLine($"{count} copies added successfully");
        }

        // method to add edition
        private void AddEdition()
        {
            Console.WriteLine("Need to view books first ? (Y/N)");
            string choice = Console.ReadLine()?.Trim() ?? "";

            if (choice.ToUpper() == "Y")
            {
                BrowseCatalog();
            }
            BookEdition edition = new BookEdition();
            System.Console.WriteLine("Enter book id");
            edition.BookId = getIntInputNoRange();
            System.Console.WriteLine("Enter edition number");
            edition.EditionNumber = getIntInputNoRange();
            System.Console.WriteLine("Enter edition label");
            edition.EditionLabel = Console.ReadLine()?.Trim() ?? "";
            System.Console.WriteLine("Enter publisher :");
            edition.Publisher = Console.ReadLine()?.Trim() ?? "";
            edition.PublishedYear = Convert.ToInt16(getIntChoice(2026, "Enter the published year"));
            edition = bookService.AddEdition(edition);
            if (edition == null)
            {
                System.Console.WriteLine("Failed to add an edition ");
                return;
            }
            System.Console.WriteLine("Edition added successfully");
        }

        // method to addbook
        private void AddBook()
        {
            Book book = new Book();
            System.Console.WriteLine("Enter title of book");
            book.Title = Console.ReadLine()?.Trim() ?? "";
            System.Console.WriteLine("Please enter author name");
            book.Author = Console.ReadLine()?.Trim() ?? "";
            ICollection<BookCategory> categories = bookService.GetCategories();
            int categoryCount = categories.Count;

            System.Console.WriteLine("Categories");

            foreach (var category in categories)
            {
                System.Console.WriteLine($"{category.CategoryId} : {category.CategoryName}");
            }
            book.CategoryId = Convert.ToInt16(getIntChoice(categoryCount, "Select category using its id:"));
            book = bookService.AddBook(book);
            if (book == null)
            {
                System.Console.WriteLine("Failed to create the book .\nPlease try again");
            }
            else
                System.Console.WriteLine("Book added successfully");

        }

        private void SmartSearchBooks()
        {
            System.Console.WriteLine("Please enter any detail you know about your book (Title/Category/Author)");
            string searchtext = Console.ReadLine()?.Trim() ?? "";
            ICollection<Book> books = bookService.SmartSearchBooks(searchtext);
            DisplayBooks(books);
        }

        private void SearchBookByCategory()
        {
            System.Console.WriteLine("Please enter the Category of your interest");
            string category = Console.ReadLine()?.Trim() ?? "";
            ICollection<Book> books = bookService.SearchBooksByCategory(category);
            DisplayBooks(books);
        }

        private void SearchBookByAuthor()
        {
            System.Console.WriteLine("Please enter the Author name");
            string author = Console.ReadLine()?.Trim() ?? "";
            ICollection<Book> books = bookService.SearchBooksByAuthor(author);
            DisplayBooks(books);
        }

        private void SearchBookByTitle()
        {
            System.Console.WriteLine("Please enter the title");
            string title = Console.ReadLine()?.Trim() ?? "";
            ICollection<Book> books = bookService.SearchBooksByTitle(title);
            DisplayBooks(books);
        }
        // helper to display books
        private void DisplayBooks(ICollection<Book> books)
        {
            if (books.Count == 0)
            {
                System.Console.WriteLine("No books found");
                return;
            }
            foreach (var book in books)
            {
                System.Console.WriteLine($"\n Book Id :{book.BookId}");
                System.Console.WriteLine($"Title : {book.Title}");
                System.Console.WriteLine($"Author : {book.Author}");
                System.Console.WriteLine($"Category : {book.Category?.CategoryName ?? "Unknown category"}");

            }
        }
        // show book analytics
        private void BrowseCatalog()
        {
            ICollection<Book> books = bookService.GetAllBooks();

            if (books.Count == 0)
            {
                Console.WriteLine("No books available");
                return;
            }

            foreach (var book in books)
            {
                Console.WriteLine($"\nBook Id : {book.BookId}\nTitle : {book.Title}\nAuthor : {book.Author}\nCategory : {book.Category?.CategoryName}");
                ICollection<BookEdition> editions = book.BookEditions.ToList();
                Console.WriteLine();
                Console.WriteLine("Editions:");
                foreach (var edition in editions)
                {
                    Console.WriteLine($"- Edition {edition.EditionNumber} {edition.EditionLabel}");
                }

                ICollection<BookCopy> copies = editions
                    .SelectMany(e => e.BookCopies)
                    .ToList();

                int available = copies.Count(c =>
                        c.CopyStatus == "Available");

                int borrowed = copies.Count(c =>
                        c.CopyStatus == "Borrowed");

                int damaged = copies.Count(c =>
                        c.CopyStatus.Contains("Damaged"));

                int lost = copies.Count(c =>
                        c.CopyStatus == "Lost");

                Console.WriteLine($"\nAvailable : {available}\nBorrowed : {borrowed}\nDamaged : {damaged}\nLost : {lost}");
            }
        }
        // member menu
        private void ManageMembers()
        {
            while (true)
            {
                Console.WriteLine("\nMember management console\n");
                Console.WriteLine("1.Add Member\n2.View Members\n3.Search Member\n4.Deactive member\n5.Back");
                int choice = getIntChoice(5, "Please enter your choice of service");
                switch (choice)
                {
                    case 1:
                        Member member = new Member();
                        GetMemberDetailsAndAdd(member);
                        break;

                    case 2:
                        ViewMembers();
                        break;
                    case 3:
                        SearchMember();
                        break;
                    case 4:
                        DeactivateMember();
                        break;
                    case 5:
                        return;

                }
            }

        }

        // search books submenu
        private void SearchBooksMenu()
        {
            while (true)
            {
                Console.WriteLine("\nSearch Books\n1.By Title\n2.By Author\n3.By Category\n4.Smart Search (give anything you know (category/title/author))\n5.Back");
                int choice = getIntChoice(5, "Enter choice");

                switch (choice)
                {
                    case 1:
                        SearchBookByTitle();
                        break;
                    case 2:
                        SearchBookByAuthor();
                        break;
                    case 3:
                        SearchBookByCategory();
                        break;
                    case 4:
                        SmartSearchBooks();
                        break;
                    case 5:
                        return;
                }
            }
        }
        // book Inventory management
        private void InventoryManagement()
        {
            while (true)
            {
                Console.WriteLine();

                Console.WriteLine("Inventory Management\n1.View All Copies\n2.View Available Copies\n3.View Borrowed Copies\n4.View Damaged Copies\n5.View Lost Copies\n6.Back");

                int choice = getIntChoice(6, "Enter your choice");

                switch (choice)
                {
                    case 1:
                        PrintBookCopies(bookService.GetAllCopies(), "All Copies");
                        break;
                    case 2:
                        PrintBookCopies(bookService.GetCopiesByStatus("Available"), "Available Copies");
                        break;
                    case 3:
                        PrintBookCopies(bookService.GetCopiesByStatus("Borrowed"), "Borrowed Copies");
                        break;
                    case 4:
                        ICollection<BookCopy> damagedCopies = bookService.GetAllCopies()
                            .Where(c => c.CopyStatus.Contains("Damaged"))
                            .ToList();

                        PrintBookCopies(damagedCopies, "Damaged Copies");
                        break;
                    case 5:
                        PrintBookCopies(bookService.GetCopiesByStatus("Lost"), "Lost Copies");
                        break;
                    case 6:
                        return;
                }
            }
        }
        private void DeactivateMember()
        {
            System.Console.WriteLine("Need to view members first? Y/N");
            string choice = Console.ReadLine()?.Trim().ToUpper() ?? "";
            if (choice == "Y") ViewMembers();
            System.Console.WriteLine("Enter your member Id");
            int memberId = getIntInputNoRange();
            System.Console.WriteLine("Enter your registered phone number");
            string phone = Console.ReadLine()?.Trim() ?? "";
            Member? member = memberService.DeactivateMember(memberId, phone);
            if (member == null) System.Console.WriteLine("Deactivation of member with id " + memberId + " failed");
            else
            {
                System.Console.WriteLine($"Member with Id:{memberId} deactivated successfully");
            }
        }

        // helper to search members 
        private void SearchMember()
        {
            System.Console.WriteLine("Enter any details you know about member (Name / Email)");
            string searchtext = Console.ReadLine()?.Trim() ?? "";
            Member? member = memberService.SearchMember(searchtext);
            if (member == null)
            {
                System.Console.WriteLine("Member with the given details not found");
                return;
            }
            System.Console.WriteLine($"{member.MemberId}  :   {member.FullName} : {member.MembershipType.TypeName} Email id : {member.Email} Activation status : " + (member.IsActive ? "Active" : "Inactive"));

        }

        // method to view all members in application
        private void ViewMembers()
        {
            ICollection<Member> members = memberService.GetAllMembers();
            if (members.Count == 0)
            {
                Console.WriteLine("No members yet in the app");
                return;
            }
            Console.WriteLine("Members in our Platform :\n");

            foreach (var member in members)
            {
                System.Console.WriteLine($"{member.MemberId}  :   {member.FullName} Membership Type: {member.MembershipType.TypeName} Email id : {member.Email} Activation status : " + (member.IsActive ? "Active" : "Inactive") + "\n");
            }
        }

        // method to get member details from user
        private void GetMemberDetailsAndAdd(Member member)
        {
            System.Console.WriteLine("Please Enter name of the member : ");
            member.FullName = Console.ReadLine()?.Trim() ?? "";
            System.Console.WriteLine("Please Enter email of the member : ");
            member.Email = Console.ReadLine()?.Trim() ?? "";
            System.Console.WriteLine("Please Enter phone of the member : ");
            member.Phone = Console.ReadLine()?.Trim() ?? "";
            System.Console.WriteLine("Please Enter address of the member : ");
            member.Address = Console.ReadLine()?.Trim() ?? "";
            member.DateOfBirth = getDOB();
            Console.WriteLine("Membership Types");
            // get all membership types
            ICollection<MembershipType> membershipTypes = memberService.GetMembershipTypes();
            int membershiptypeCount = membershipTypes.Count;

            foreach (var type in membershipTypes)
            {
                Console.WriteLine($"{type.MembershipTypeId} : " + $"{type.TypeName}");
            }

            member.MembershipTypeId = Convert.ToInt16(getIntChoice(membershiptypeCount, "Please enter membershiptype Id"));
            // dummy approving text 
            System.Console.WriteLine("Processing your request .......");
            System.Console.WriteLine("Verified your eligibility for enrollment for the membership of " + membershipTypes.SingleOrDefault(mt => mt.MembershipTypeId == member.MembershipTypeId)?.TypeName);
            member.IsActive = true;
            member = memberService.AddMember(member);
            System.Console.WriteLine("Member added successfully " + member);

        }

        private DateOnly? getDOB()
        {
            Console.Write("Enter your Date of Birth (e.g., dd-MM-yyyy): ");
            DateOnly dob;
            while (!DateOnly.TryParseExact(Console.ReadLine(), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dob) || dob >= DateOnly.FromDateTime(DateTime.Today))
            {
                Console.WriteLine("Enter a valid date in format dd-MM-yyyy and your real Date of Birth !!");
            }
            return dob;
        }

        // helper to exit app
        private void exitAppHelper()
        {
            System.Console.WriteLine("Exiting app .......\n See you soon");
            exitApp = true;
        }

        //method to get a valid int choice with the specific range
        private int getIntChoice(int range, string prompt)
        {
            Console.WriteLine(prompt);
            int choice;
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > range)
            {
                System.Console.WriteLine("Please enter a valid choice");
            }
            return choice;

        }
        // method to display Main Menu
        private void displayMainMenu()
        {
            Console.WriteLine("1.Member Management\n2.Book Management\n3.Borrow Book\n4.Return Book\n5.Fine Management\n6.Reports\n7.Exit");
        }
    }
}