# TODO

1. Controllery layer

   - pepa

1. Executor implementace

   - plus managment - paralelne
   - tomas
   TODO:
		* podpora pro peridicke spousteni jedne instance exekutoru

1. Model layer

   - SQL lite - MILAN

1. Knihovna na visualizaci Exekutoru / grafu

1. GraphQL

1. Seznam exekutoru - CRUD

1. Docker

## Endpoints

1. `/record/{id}` - GET

1. `/records` - GET

1. `/record` - POST s `Json`

   - save new

1. `/record/{id}` - PATCH s `Json`

   - update existing

1. `/record/{id}/run` - POST  <<<--- TODO, not sure how to implement

   - run existing

1. `/record/{id}` - DELETE
	 
	- delete existing

# POZNAMKY

- SQLlite - entity framework
- REACT bootstrap

## Navigace
Asi zatim prazdno.
1. Home
1. About us

## Paginated View - seznam 
### Active selection

### Cudliky doleva - doprava
na posun stranek

### Search
podle:
* Label

### Filter
podle:
* URL
* Label
Tags

### Sort
podle:
* URL
* Last execution time

### Item
ma:
* Label
* Periodicity
* Tags
* Last execution
* status of last execution - puntiky na levo
	* ceka na zpracovani
	* aktualne bezi
	* dobehal uspense
	* dobehla zrusenim
* RUD
	* tri tecky napravo

## ShowGraph cudlik

## Create cudlik
Otevre modalni okno. 

Uzivatel musi zadat:
1. Label
1. Url
1. Periodicity
1. Regex

# Visualizace grafu
Nekdy pak.

# Backend

- GraphQL

# Deployment

- Docker

# JOBS
## Milan a Pepa:
1. Paginated view
	* hlavni kostra
	* jak to pujde

## Tomas
1. Create WebsiteRecord modal window
1. Dodelat executionera


# Another meeting

- pristi tyden nekdy - streda?