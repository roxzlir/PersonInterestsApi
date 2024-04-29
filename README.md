# API solution for adding Person's with different interests with possibility to add links to those interests.

## PersonInterestsApi

### GET all persons with API:
#### /persons

### GET all interest for specific person with API:
#### /persons/interests/{personId}

### GET all links added form a specific person with API:
#### /all/links/added/by/person/{personId}

To add a new interest you first need to add that interest:
### POST a new Interest with API:
#### /interests
## NOTE: If there already is a interest you like, you can skip the POST to /interest
and then you can connect that interest to you,
### POST a new Interest with API:
#### /personinterests

There is also possible to add a new Person and Interest at the same time:
### POST a new Person with Interest with API:
#### /persons/interests

### POST a link to existing Person and Interest API:
#### /persons/{personId}/interests/{interestId}/links



## OTHER FEATUERS
### GET all interest and links for one person in hierarchical order
#### /interests/links/for/person/{personId}

### GET all persons that "contains" the letters in there names that you searching for
#### /searchForPerson/{name}/info

### GET a paginated result based on what you want
#### /persons/paginate?pageSize=1&pageNumber=10

### GET all persons with all there interest and links connected to those interests
#### /allpersoninfo

### GET all interests with all the links that are connected to them
#### /links/on/interests

### GET a paginated result on the allpersoninfo result
#### /allpersoninfo/paginate?pageSize=10&pageNumber=1

### POST a new link
#### /links

### POST a new person
#### /persons
