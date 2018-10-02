# ochs

Open Competition HEMA Software

At the moment OCHS runs as a self hosted console application or windows service.
As soon as you start the console application (ochs.exe) it starts to listen to the url configured in ochs.exe.config (default http://*/ochs/) and in the console it shows the urls it is listening to.
If you start it without a database configuration (configured in hibernate.cfg.xml) it will use SQLite by default and create the databse and show you the generated admin password in the console.
With your browser go the url (http://localhost/ochs/) and you see the start page.
The first time login with the user "Admin" and the password shown in the console.
