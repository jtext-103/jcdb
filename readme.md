
# J-TEXT Cloud Database (JCDB)

## What is J-TEXT cloud Database

JCDB is a full stack database system for pulse based fusion expeirment disgnostic signal storage. 

It is kinda like MDSplus, but with some major difference:

1. JCDB is designed with large data first principle, it is based on cluster and scale out deisgns.

2. JCDB is not a file format not a file system. It is a service. A serve deployed on a NoSQL database cluster that provide read/wirte service of the experiment data as well as its metadata and querying of the metadata.

3. There are 2 set of APIs one api is for high performance usage that connect to the cluster directly; the other is for non-performance critical applications and is a set of RESTful Web APIs.

## How to use

Clone the code, and find deisgn and develop docs in the design folder. you will need basic knowldge of cassandra, mongodb, and C# to use is.

## Status

Although with expert helps you can delploy JCDB and develop data aquisition system to work with it. But it is not recommand to use it in production system.

JCDB is mainly a demostration and proof of concept. You can use the code in this project to make further developments.