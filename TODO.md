# OTAZKY a TODO na meeting
## Home
1. jak udelat, aby paginatedView poslal selected ids do rodice - home.js, ktery to uz jen pak preposle routingem do graf vizualizace
	* callback onSelected, ktery prida id selected row do selectedIds
1. Editace dodelat
	* nezapomenout na refresh dat
1. Delete dodelat
	* nezapomenout na refresh dat
1. filtrace a hledani je done?
1. Souvisi activate a deactive se statusem?
1. cudliky lepsi styling
	* lepe umistit
	* nemusi se posunout nahoru spolu s tabulkou, mozna ani ta tabulka se nemusi takhle zmensit?

## WEBSITE RECORD DETAILS
* novy route
* kompletne totiz chybi executiony, by melo byt pod kazdym recordem nekde ...

## BACKEND
1. otestovani periodicity, mam pocit ze nefunguje ...
1. domimplementovani delete / update / run recordu na controlleru

## GRAF VISUALIZACE
1. live graf update
1. moznost startovat exekuce z website recordu z graf view
	* MODALNI OKNO 
1. moznost vytvorit novy website rekord z graf view
	* zmenit to co je momentalne onClick na onHover na grafu
	* onClick se ukaze MODALNI OKNO
1. cudliky na graf vizualizace lepsi styling
	* celou dobu pekne pod sebou v pravem hornim rohu
1. toggle na graf vizualizace se chova nejak debilne, asi jsem spatne pouzil react state?

## MODALNI OKNO V GRAF VIZUALIZACE PRO NODE
* node details
* seznam website records
* moznost prejit na WEBSITE RECORD DETAILS

## NA KONEC
1. graphQL API
1. Docker
