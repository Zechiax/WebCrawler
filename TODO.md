# TODO
## NA KONEC
1. graphQL API
1. Docker

## DODELAT
### Milan
* View graph na recordu ktery nema graf spadne (show sad face on graph view)
* select a activate/deactivate nefunguje (chyba v databazi)
* chybi implementace live view grafu na pozadi nejakym job thread
	* idealne jenom nalepovat nodes a edges a ne ho cely renderovat znovu - tahat cela data ze serveru nevadi

#### Done
* validace regex
* Delete hazi vyjimku - asi to kaskadovani stale?
* edit needituje

### Pepa
* nefunguje search na tags
* udelat refresh tabulky po create / delete / edit aniz bych musel refreshovat stranku nebo tahat vsechny Records ze serveru
* v graph view chybi toggle mezi domain a websites view
* v graph view chybi toggle mezi static a live view
* v graph view chybi cudlik pro live update pokud ve static view, zmizi pokud v live view

### Tomas
* nemuzu klikat na nodes v grafu a videt info a moznost create new record
* nejsou propejene selected graph ids co vyberu ve websites records tabulce s graph view

### Optional
* live status update o statusu recordu
* last run time u recordu
* pocet nalezenych nodes
