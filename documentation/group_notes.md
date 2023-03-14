# Group notes / Reflections

## 07/02/2023

What and why use containers?

* Smart, as it makes sure that the software will run on any "machine".
    * Consistancy, file systems...  
*

## 23/02/2023

We discussed the pipeline at the lecture on the 21st, and continued the discussion today.
* We have created a designated development branch, the merge branch for code before it goes to main and is deployed.
* We have also discussed the potential for deploying code to "test" droplets before it is fully released/deployed.

Then we went over the things we need to get done in order to be ready for the simulator api going live next tuesday (28th of feb).
* We still need to (potentially) have a vagrantfile that is able to launch our minitwit app and simulator api, although we have discovered that there is an action for GithubActions that can deploy straight to digital ocean. 
* We might still need some docker images for our project (we have a simple images atm).
* We need to figure out the database situation, as SQLite has become quite an issue when trying to containerize our project. We will try to migrate to a Postgres database (possibly a managed one by digital ocean).