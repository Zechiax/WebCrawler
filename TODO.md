# TODO
## NA KONEC
1. graphQL API
1. Docker

## DODELAT
* View graph na recordu ktery nema graf spadne
* select a active nefunguje
* ukazat v record info jestli ma graf nebo ne (opt)
* nefunguje search na tags
* validace regex
* Delete hazi vyjimku - asi to kaskadovani stale?
* edit needituje
* nemuzu klikat na nodes v grafu a videt info a moznost create new record
* nejsou propejene selected graph ids co vyberu ve websites records tabulce s graph view
* udelat refresh tabulky po create / delete / edit aniz bych musel refreshovat stranku nebo tahat vsechny Records ze serveru
* v graph view chybi toggle mezi domain a websites view
* v graph view chybi toggle mezi static a live view
* v graph view chybi cudlik pro live update pokud ve static view, zmizi pokud v live view
* chybi implementace live view grafu na pozadi nejakym job thread
	* idealne jenom nalepovat nodes a edges a ne ho cely renderovat znovu - tahat cela data ze serveru nevadi
