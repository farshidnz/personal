Feature: PausedMerchantsMobilePopularPage

https://shopgoau.atlassian.net/browse/CPS-2621

@tag1
Scenario: Popular Stores Paused Merchants in Shop Page for Mobile
	Given Merchant Pause Feature Flag status is '<flag status>'
	When Shop page is called and Merchant Pause status is '<pause status>'
	Then Resulting list of merchants does '<display>' contain merchants with pause status true

Examples: 
	| flag status | pause status | display |
	| off         | false        |		   |
	| off         | true         |         |
	| on          | false        |  	   |
	| on          | true         | not     |
