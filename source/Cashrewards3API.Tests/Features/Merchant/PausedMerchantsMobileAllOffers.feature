Feature: PausedMerchantsMobileAllOffers

https://shopgoau.atlassian.net/browse/CPS-1582

@tag1
Scenario Outline: Paused Merchants in All Offers page for Mobile
	Given feature flag for Merchant Pause is '<flag status>'
	When all merchants is called and Merchant Paused status is '<pause status>'
	Then  the resulting list does '<display>' contain merchants with pause status true 
	
Examples:
	| flag status | pause status | display |
	| off         | false        |		   |
	| off         | true         |         |
	| on          | false        |  	   |
	| on          | true         | not     |
