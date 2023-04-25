# Investigation

## CPU

~2% for the sim api droplet.
The database is currently consuming 40% of memory, ~8% CPU usage.

A potentially interested party could be the operations department (operators). 

## Response time

We were unable to find the average response time of the sim API.


Department: Developers and operators.

## Amount of users
9471 users were in the database

Department: business department.

## Average amount of followers pr. user
```sql
select avg(c)
from (select count(whom_id) as c
     from follower
     group by whom_id) as x

```
24,6

Department: business, 

## which role(s) are we? whats the most important metric

1. cpu load - if we hit 100% service stops working
2. roles - all, but primarily operator and developers.