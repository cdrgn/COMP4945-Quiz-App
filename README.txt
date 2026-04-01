TRIVIA APP SETUP
================

STEP 1: START ORACLE DATABASE
------------------------------
docker run -d --name oracle23c -p 1521:1521 -e ORACLE_PWD=password container-registry.oracle.com/database/free:latest

Wait 2-3 minutes for Oracle to start. Check with:
docker logs oracle23c

Wait for "DATABASE IS READY TO USE!"


STEP 2: CONFIGURE DATABASE CONNECTION
--------------------------------------
Edit: TriviaAPI/appsettings.json

Update this line if needed:
"DefaultConnection": "User Id=SYSTEM;Password=password;Data Source=localhost:1521/FREEPDB1"

- Password: Must match what you set in Step 1 (-e ORACLE_PWD=password)
- Port: 1521 (change if you used different port)
- Service Name: FREEPDB1 (might be XE or FREE on some systems)


STEP 3: RUN THE APPLICATION
----------------------------
Terminal 1 (Backend):
    cd TriviaAPI
    dotnet run

Terminal 2 (Frontend):
    cd trivia-react
    npm install
    npm start


LOGIN CREDENTIALS
-----------------
Admin: admin@hotmail.com / password
User:  user@hotmail.com / password


DAILY WORKFLOW
--------------
1. Start Oracle: docker start oracle23c
2. Terminal 1: cd TriviaAPI && dotnet run
3. Terminal 2: cd trivia-react && npm start


TROUBLESHOOTING
---------------
Oracle not running?
    docker ps
    docker start oracle23c

Need to reset?
    docker stop oracle23c
    docker rm oracle23c
    (Then re-run Step 1)