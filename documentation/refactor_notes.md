# Refactor notes - week 2

## Feature Mapping
MiniTwit features:

* MiniTwit.py

Database:

* Connect to database
* Init database
* Login
* Log out
* Register


### API:

Timeline at “/”
* If user is not logged → redirect to “/public
* Display latest messages from all users
    * if user is logged in → check db for who follows whom.
* Display certain user’s tweets

User profiles at “/{username}”
* check db for user
    * if !exists → return noget 404
    * if exists → render timeline.html with tweets from {username} and active user from session.

Follow a user at “/{username}/follow”
* adds the current (active) user as a follower for the {username}
* if not logged in → return 401
* if no such {username} exists → return 404
* then add active user as a follower to {username} in db.
    and render timeline at “/{username}”

Remove current user as follower of other user at “/{username}/unfollow
* if not logged in → 401
* if {username} not found → 404
* else unfollow in db and render timeline at “/{username}
Add messages at “/add_message”
* HTTPS POST
* if user not logged in → return 401,
    then create a new message to the database.
* Registers a message for current user 



## Refactor Log
We decided to go with Blazor for our solution.
When re-using the python test suite we tried to use/understand different http libraries, “requests” was the one we got the work, after many attempts. We replaced the “assert” python keyword with self.assert because assert didn’t show us both expected and actual input for the test.
We had trouble comparing strings in python. All tests would suddenly pass when using the __eq__ function.

We encountered a  problem with converting time from an integer to time, but after some  research we found out that we had to use the date 1970, jan 1 to translate the integer into Datetime.   
```c# 
PubDate =  new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(s.GetInt32(3))
```


We had a problem with Gravatar icons, turns out the hash should be converted to lowercase.

The application is running a bit slow. We expect it is to do with overhead in our handling of data. Everytime we read a twit from the database we are sending the twit and an author object to the frontend, meaning a lot of identical Author objects are being transferred.
Furthermore, our api is currently establishing a new connection to the database every time it is called.

We have had trouble with Blazor routing. We have split the timeline page into three separate razor pages. 

Left to do:
* Follow/unfollow users.
* Add messages/create twits(write to database, fix message form).
* Make sure Tests work
* Split the frontend and backend port??
    * We used the --hosted flag. “api” and front are current on the same port.
    * So, API routes have to be different from frontend ones.
* EFCore Refactor





## Log 13-2-23
* Created the follow/unfollow box depending on the user logged in.
* Created a post method for following a user.
    * it returns an Ok - 200 instead of the expected 201 from a post request.
* Created a post method for unfolllowing a user (BAD)
    * It should not be a post but a delete instead, and we should look at the return codes aswell.

* Inserted Journal: Wal into our sql connection in program.cs in /Server. to fix sqlite lock issues.    
    * We had issue with the database being "locked", returning a errorcode 5.


We are trying to fix to our forms, by inserting a model (message submit model client/shared) to make add message work

### 13/02/2023
Having trouble with the API and making requests fit the expected format.
Inserting with SQLite is a pain.

### 16/02/2023
Implementing the first iteration of our sim_api.
* currently it is just a copy of the python version except it is C# code. Possibly not the fastest cleanest or mode C#-ish style.
* the Python objects work different from C# objects, so for the fllws Post method, we have had to introduce a record that have to nullable members, if one (follow or unfollow) is null, we know that the request is targeting the other action (follow or unfollow). 
* Also, understanding the python fllws endpoint was quite confusing at first, but if you imagine that the {username} is who is acting, it kind of makes sense.

 
