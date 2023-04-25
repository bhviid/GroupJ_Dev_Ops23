# Postmortems

## Week 8 - catching a live bug by looking at logs

### 🐞 The bug

The bug caused unfollow requests from the simulator API to be handled incorrectly. When the simulator API requests to unfollow for some user the system is supposed to remove a database record, but the system was instead trying to insert a new record with the same exact values as the existing one.

### 🔎 Catching the bug

On our Elastic search dashboard we were able to see that our simulator API was sending a lot of *Internal Server Errors* (500s). Using the filter features of Elastic search we were able to pinpoint the specific endpoint which sent most of the server errors and look through the corresponding unhandled Exceptions (C#). Apperently, our endpoint was violating the unique constraint on primary keys when trying to insert database records. With this knowledge we looked at the code and realized our "unfollow" branch of the method was trying to insert records instead of removing them.
After a fix was implemented the amount of logged *Internal Server Errors* fell drastically.
