# TODO
## NA KONEC
1. graphQL API
1. Docker


## TODO Domluvene
## PEPA
1. cudlik view graph do tabulky nahoru

1. active / inactive
	* na active zapnu crawl = dam do fronty periodic executioner manageru
		* zavolat na controlleru Controller.RunCrawler()
	* na deactive stopnu job podle jobId
		* zavolat Controller.StopController()

1. cudliky na graf vizualizace lepsi styling
	* css
	* celou dobu pekne pod sebou v pravem hornim rohu

1. refresh tabulky automaticky po pridani noveho recordu

1. Editace dodelat
	* nezapomenout na refresh dat
	* zavolas Controller.UpdateRecord()

1. Delete dodelat
	* nezapomenout na refresh dat
	* zavolas Controller.DeleteRecord() na controlleru

## TOMAS
### Modalni okno
1. naimplementovat controller Controller.RunCrawler() a Controller.StopCrawler() - done

1. label nemusi byt unikatni
- done

1. picnout records do modalniho okna po kliknuti na node
	* ja jen pak picnu pepovu komponentu do seznamu 
	- done

1. moznost vytvorit novy website rekord
	* zmenit to co je momentalne onClick na onHover na grafu
	* onClick se ukaze MODALNI OKNO
	- done

1. otestovani periodicity, mam pocit ze nefunguje ...
	- done , funguje

1. toggle na graf vizualizace se chova nejak debilne, asi jsem spatne pouzil react state?
- done

1. zkusit pri vice datech tu simulaci neauktualizovat a pockat az se nejak ustali ten graf do finalni pozice

## MILAN
✅ 1. domimplementovani delete / update recordu na controlleru

1. dat validaci nekam jinam, ne do controlleru - navic, spis neni treba imo

1. filtrace a hledani - presne to co ma skoda na strankach - nic vin a nic min
	* inactive / active filtrovani - jenom nejaky switch

1. live graf update

1. zkusit pri vice datech tu simulaci neauktualizovat a pockat az se nejak ustali ten graf do finalni pozice
	* merge()

1. graf se po dokonceni nemusi ulozit ke spravnemu website recordu + proc jsou outgoingWebsites vzdy prazdne pri /Records ?
	* poslat do PeriodicExecutionManager.EnqueForCrawl(...) record id, treba skrze CrawlInfo (+CrawlInfo.RecordId)
	* pak Crawler pri ukladani do databaze ma na ruce RecordId
	* oni totiz ty jobId se resetuji po kazde co se resetuje server

# DEADLINE: 19.06.
