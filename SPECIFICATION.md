WebCrawler
==========

The objective is to implement a [Web crawler](https://cs.wikipedia.org/wiki/Web_crawler) with a web-based interface.

Site management
---------------

The application should allow a user to keep track of _website records_ to crawl. For each _website record_ the user can specify:

* _URL_ - where the crawler should start.
* _Boundary RegExp_ - when the crawler found a link, the link must match this expression in order to be followed. User is required to provide value for this.
* _Periodicity_ (minute, hour, day) - how often should the site be crawled.
* _Label_ - user given label.
* _Active / Inactive_ - if inactive, the site is not crawled based on the _Periodicity_.
* _Tags_ - user given strings.

The application should implement common [CRUD](https://cs.wikipedia.org/wiki/CRUD) operation. The user can see _website records_ in a paginated view. The view can be filtered using _URL_, _Label_, and/or _Tags_. The view can be sorted based on the URL or the last time a site was crawled. The view must contain _Label_, _Periodicity_, _Tags_, time of last _execution_, the status of last _execution_.

Execution management
--------------------

Each active _website record_ is executed based on the periodicity. Each execution creates a new _execution_. For example, if the _Periodicity_ is an hour, the executor tries to crawl the site every hour ~ last _execution_ time + 60 minutes. You may use start of the last execution or end of the last execution. While doing the first may not be safe, id does not matter here. If there is no _execution_ for a given record and the record is active the crawling is started as soon as possible, this should be implemented using some sort of a queue. A user can list all the _executions_, or filter all _executions_ for a single _website record_. In both cases, the list must be paginated. The list must contain _website record_'s label, _execution_ status, start/end time, number of sites crawled. A user can manually start an _execution_ for a given _website record_. When a _website records_ is deleted all _executions_ and relevant data are removed as well.

Executor
--------

The executor is responsible for executing, i.e. crawling selected websites. Crawler downloads the website and looks for all hyperlinks. For each detected hyperlink that matches the website record _Boundary RegExp_ the crawler also crawls the given page. For each crawled website it creates a record with the following data:

* _URL_
* _Crawl time_
* _Title_ - page title
* _Links_ - List of outgoing links

Crawled data are stored as a part of the _website record_, so the old data are lost once the new _execution_ is successfully finished. It must be possible to run multiple _executions_ at once.

Visualisation
-------------

For selected _website records_ (_active selection_) user can view a map of crawled pages as a graph. Nodes are websites/domains. There is an oriented edge (connection) from one node to another if there is a hyperlink connecting them in a given direction. The graph should also contain nodes for websites/domains that were not crawled due to a _Boundary RegExp_ restriction. Those nodes will have different visuals so they can be easily identified. A user can switch between website view and domain view. In the website view, every website is represented by a node. In the domain view, all nodes from a given domain (use a full domain name) are replaced by a single node. By double-clicking, the node the user can open node detail. For crawled nodes, the details contain _URL_, _Crawl time_, and list of _website record_ that crawled given node. The user can start new _executions_ for one of the listed _website records_. For other nodes, the detail contains only _URL_ and the user can create and execute a new _website record_. The newly created _website record_ is automatically added to the _active selection_ and mode is changed to _live_. The visualisation can be in _live_ or _static_ mode. In _static_ data are not refreshed. In the _live_ mode data are periodically updated based on the new _executions_ for _active selection_.  
If a single node is crawled by multiple _executions_ from _active selection_ data from lates _execution_ are used for detail.  
Use page title or URL, in given order of preference, as a node label. In domain node employ the URL.

API
---

The _website record_ and _execution_ CRUD must be exposed using HTTP-based API documented using OpenAPI / Swagger. Crawled data of all _website records_ can be queried using GraphQL. The GraphQL model must "implement" the following schema:

```
    type Query{
        websites: [WebPage!]!
        nodes(webPages: [ID!]): [Node!]!
    }
    
    type WebPage{
        identifier: ID!
        label: String!
        url: String!
        regexp: String!
        tags: [String!]! 
        active: Boolean!
    }
    
    type Node{
        title: String
        url: String!
        crawlTime: String
        links: [Node!]!
        owner: WebPage!
    }
```

Deployment
----------

The whole application can be deployed using docker-compose.

    git clone ...
    docker compose up

Others
------

* The application must provide a reasonable level of user experience, be reasonably documented with reasonable code style.
* No documentation is required, but you will be asked to showcase and comment on the final software.
* When scraping a site follow only `<a href="..." ...`.
* The scraping parallelism must utilize more then one thread. It is not sufficient, for the purpose of the assignment, to employ just NodeJS and argument of async IO. For example, NodeJS support worker threads - using them for crawling is ok.
