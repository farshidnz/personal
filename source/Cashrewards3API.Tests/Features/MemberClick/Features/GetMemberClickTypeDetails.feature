Feature: GetMemberClickTypeDetails

@MemberClick
Scenario Outline: Get merchant/offer/merchanttier details based on memberclick hyphenated string
	Given a member makes a click
	When memberclicktypehyphenatedstring is <hyphenatedString>
	Then the merchant hyphenated string is <merchantHyphenatedString> and merchantpaused is <pausestatus> 
	 
	Examples:
	| hyphenatedString                   | merchantHyphenatedString | pausestatus |
	| cashmere-boutique-m                | cashmere-boutique        | true        |
	| cashmere-boutique-1234-mt          | cashmere-boutique        | true        |
	| red-rooster-m                      | red-rooster              | false       |
	| red-rooster-1234-mt                | red-rooster              | false       |
	| make-a-booking-255373-o            | booking-com              | true        |
	| get-it-all-at-amazon-com-444555-o  | amazon-australia         | false       |

@MemberClick
Scenario Outline: Get Exception when memberclick hyphenated string is invalid
	Given a member makes a click
	When memberclicktypehyphenatedString is <invalidHyphenatedString>
	Then an exception is thrown

	Examples:
	| invalidHyphenatedString |
	|                         |
	| liquorlan-m             |
	| liquorland-              |
	| liquorland              |
