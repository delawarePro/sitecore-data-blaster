Sitecore DataBlaster is a **low-level API to rapidly load lots of data in a Sitecore database** while still being compatible with other moving parts like caches, indexes and others. Loading a lot of data in Sitecore is typically quite slow. Although Sitecore allows you to perform some performance tweaks, like e.g. BulkUpdateContext or EventDisabler, the  central item APIs are not optimized for batch/bulk updates.

## What do I use it for?
* **Deserialization** of developer managed content (see below for integration with **Unicorn**)
* Standup fresh Sitecore environments for **Continuous Integration**
* Full and delta **imports** into Sitecore
* **Transfer items** from one db to another when upgrading Sitecore versions

## How does it work?
A stream (IEnumerable) of items is bulk copied (SqlBulkCopy) into a temp table after which an elaborate SQL merge script is executed to insert, update and delete items and fields in the database. During the merge, the changes are tracked, so that it can report the exact item changes. These changes are then used to synchronize other parts like e.g. caches and indexes.

## Are you crazy? What if Sitecore changes its database schema?
First of all, the database schema hasn't changed significantly in ages. We've been running these scripts from Sitecore 7.1 onwards. Second, database schema changes will have quite some impact on Sitecore as well, so they'll probably be quite careful with this.

## Are you sure? Is this proven technology?
We, at [delaware digital](http://digital.delawareconsulting.com), successfully use this approach for a **couple of years** now. Our own serialization tools were build on this library. By using this approach in a specific project, our deserialization time decreased from arround **50+ minutes to less than 1 minute**.

Next to that, we are strong believers in continuous integration and continous delivery, so **automated testing is crucial** for us. Without this library, automated **integration, web and smoke tests wouldn't really be feasable** on the large projects that we're building and supporting.

## Alright, so what breaks when I use this?
The data blaster stores everything in the database in native Sitecore format and we've gone out of our way to make sure everything is **fully supported**, like following components:
* **Bucketing**: auto bucketing items into Sitecore buckets.
* **Item API**: caches are cleared immediately after data blast.
* **Link database**: links between items are detected and updated.
* **History engine**: used to be important for index updates.
* **Publish queue**: if you want to use the incremental publish.
* **Indexes**: optimized index update with auto rebuild and refresh support.

There's still **one thing not supported yet**, which is updating the event queue. This is typically only important when you have multiple content management nodes. **However** we have an alpha version implementation that will follow soon. 

## I don't find any automated tests! Are you kidding me?
The automated tests are not located in this repository, because we need a more elaborate setup, but we run over **65 automated tests** on a real Sitecore database on every checkin.

## Unicorn you said?
[Unicorn](https://github.com/kamsar/Unicorn) is a cool (de)serialization utility and it's quite optimized for performance. However, it's not performing very well when filling 'empty' Sitecore databases. This is not Unicorn's fault, but the issue with the underlying item API. 

Because filling 'empty' Sitecore databases is typically something we do **very often**, we created, with directions of [kamsar](https://github.com/kamsar), a drop-in integration for Unicorn. Which, in our tests, is faster than the default implementation in all our cases.