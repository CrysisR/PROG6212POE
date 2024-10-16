# **The Contract Monthly Claims System: The Answer to Easy Claims Management**  
**By Tyron Jeremiah Naidoo (ST10385722)**  

## **1. Intro**  
### _**The Contract Monthly Claims System**_ aims to streamline the process of creating monthly lesson claims for lecturers who work on a monthly contract, also known as Independent Contract Lecturers.  

With this system:  
- Programme Coordinators and Academic Managers can **review**, **approve**, or **deny** submitted claims based on provided information.  
- An **Admin** can manage the application, allowing for the creation of new Programme Coordinators and Academic Managers.  

---

## **2. Application Specification**  
This application is built using **Microsoft's ASP.NET Model-View-Controller (MVC)** framework.  

**Front-end**:  
- Primary language: **C#**  
- Supplemented by **HTML**, **CSS**, and **JavaScript**  

**Back-end**:  
- **SQL database** for storage  
- **Code-first migrations** approach  

Developed in **Microsoft Visual Studio IDE**, the application's functionality is rigorously tested using the **xUnit** package from NuGet Package Manager.  

### **Steps to Connect the Front-End to the Database**  

1. **Download** or **clone** the application from the GitHub repository.  
2. Unzip the application and open it in **Visual Studio**.  
3. Navigate to the `appsettings.json` file in the root directory and paste the following code:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Server=(localdb)\\mssqllocaldb;Database=PROG6212POE;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

## **3. GitHub Link**  
To access the **GitHub repository**, follow the link below:  

👉 [GitHub Repository](https://github.com/VCWVL/prog6212-part-2-st10385722)  

If the above link doesn’t work, use an alternative link:  
[INSERT PERSONAL GIT REPO HERE]  

---

## **4. Developer Information**  
This application was **solely developed** by me, **Tyron Jeremiah Naidoo (ST10385722)**, with help from various online resources and forums.  

**Contact me** at:  
📧 naidootyron891@gmail.com  

---

## **5. Frequently Asked Questions**  

### **Q: Why can't I select my role from the register page?**  
A: Due to security concerns, this feature is restricted to an **Admin-only** page. You need to be verified as a staff member first.  

### **Q: Why does my page keep refreshing when I upload a document?**  
A: Common causes include:  
- Unsupported file types: **.pdf, .xlsx, .docx, .pptx, .png, .jpg**  
- File size exceeding **5MB**  

### **Q: How do I know if my claim is being reviewed?**  
A: All claims are stored in the database. Staff members can sort claims by status. If a claim hasn’t been addressed in **2 weeks**, please email us at **claimsreportcmcs@gmail.com**.  

### **Q: Once a user is demoted from Admin, can they be promoted again?**  
A: **No.** Demotion is a permanent decision to restrict admin privileges.  

### **Q: How do I test the application's functions?**  
A: Follow these steps to run the **unit tests**:  

1. Open the `PROG6212POETesting` file in the base `PROG6212POE` folder.  
2. Navigate to the `ClaimsControllerTesting` and `RolesControllerTesting` classes.  
3. Right-click and select **Run Test**.  
4. Wait for **30 seconds to 1 minute** for the results.  
5. Record the results.  

---

## **6. Code References**  
Big thanks to these StackOverflow contributors for their help:  
- User **Dale Alleshouse**  
- User **mvanella**  
- User **Anushka Madushan**  
- User **danludwig**  

---

## **7. Default Login Details**  

| **Role**              | **Username**            |  
|-----------------------|-------------------------|  
| Lecturer              | lecturer@test.com       |  
| Programme Coordinator | pc@test.com             |  
| Academic Manager      | am@test.com             |  
| Admin                 | admin@test.com          |  
| Super Admin           | superadmin@test.com     |  

> **Note:** All users have the following password for testing: `Adm!n1234`  

---

## **9. Change Logs**  
Recent updates based on feedback:  
- Added **claim submission functionality**  
- Improved **GitHub commit history** for version control  
- Enhanced **README** for better articulation of application features   
