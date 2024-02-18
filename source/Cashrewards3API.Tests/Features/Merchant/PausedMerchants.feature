Feature: Paused merchants

https://shopgoau.atlassian.net/browse/CPS-1583

@tag1
Scenario Outline: Paused Merchants in Merchant page for Mobile
	Given merchant is online and feature flag for Merchant Pause is '<flag status>'
	When merchant details for merchant id is called and Merchant Paused status is '<pause status>'
	Then the merchant commission value is '<commission value>' and commission string value is '<commission string value>' and merchant tiers value is '<merchant tiers>'
		
Examples:
	| flag status | pause status | commission value | commission string value | merchant tiers |
	| off         | false        | >0               | not empty               | any            |
	| off         | true         | >0               | not empty               | any            |
	| on          | false        | >0               | not empty               | any            |
	| on          | true         | 0                | empty                   | empty          |
