Feature: PausedMerchantsWebShopPopularPage

https://shopgoau.atlassian.net/browse/CPS-1376

@tag1
Scenario: Popular Stores Paused Merchants in Shop Page for Web
	Given Merchant Pause feature flag is '<flag status>'
	When shop page is called and Merchant Paused status is '<pause status>'
	Then the resulting list of merchants does '<display>' contain merchants with pause status true

Examples: 
	| flag status | pause status | display |
	| off         | false        |		   |
	| off         | true         |         |
	| on          | false        |  	   |
	| on          | true         | not     |