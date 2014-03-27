﻿Feature: Language Parser
	In order to get validation errors
	As a Warewolf user
	I want proper validation message for incorrect inputs in all the tool



Scenario Outline: Variable with scalar language parsor 
	Given I have a variable '<variable>'	
    When I validate
	Then has error will be '<error>'.
	And the error message will be '<message>'
Examples: 	
	| variable                       | error | message                                                                                                                                    |
	| [[var]]                        | false |                                                                                                                                            |
	| [[var1]]                       | false |                                                                                                                                            |
	| [[var@]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var#]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var$]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var%]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var^]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var&]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var]]00]]                    | true  | 1) Invalid syntax - You have close (}}) without related open ( [[) 2) Invalid expression: opening and closing brackets don't match         |
	| [[var]]@]]                     | true  | 1) Variable - Invalid expression: opening and closing brackets don't match 2) Variable name contains invalid character(s)                  |
	| [[var]]]]                      | true  | 1) Invalid syntax - You have close (}}) without related open ( [[) 2) Invalid expression: opening and closing brackets don't match         |
	| [[[[var]]                      | true  | 1) [[var]] does not exist in your Data List 2) Invalid expression: opening and closing brackets don't match                                |
	| [[(var)]]                      | true  | 1) [[var]] does not exist in your Data List                                                                                                |
	| [[var.()]]                     | true  | 1) [[var]] does not exist in your Data List                                                                                                |
	| [[]]                           | true  | 1) [[]]is missing a variable                                                                                                               |
	| [[()]]                         | true  | 1) [[()]] does not exist in your Data List                                                                                                 |
	| [[(1)]]                        | true  | 1) [[(1)]] does not exist in your Data List                                                                                                |
	| [[var  ]]                      | true  | 1) Variable contains a space, this is an invalid character for a variable name                                                             |
	| [[var~]]                       | true  | 1) Invalid syntax - You have close (}}) without related open ( [[) 2) Invalid expression: opening and closing brackets don't match         |
	| [[var+]]                       | true  | 1)Variable name contains invalid character(s)                                                                                              |
	| [[var]a]]                      | true  | 1) Invalid syntax - You have close (}}) without related open ( [[) 2) Invalid expression: opening and closing brackets don't match         |
	| [[var[a]]                      | true  | 1) Invalid syntax - You have close (}}) without related open ( [[) 2) Invalid expression: opening and closing brackets don't match         |
	| [[var[[a]]]]                   | true  | 1) Invalid syntax - You have close (}}) without related open                                                                               |
	| [[var[[]]                      | true  | 1) Invalid expression: opening and closing brackets don't match                                                                            |
	| [[var[[1]]]]                   | true  | 1) Invalid expression: cant start a variable name with a number                                                                            |
	| [[var.a]]                      | true  | 1) [[var]] does not exist in your Data List                                                                                                |
	| [[var1.a]]                     | true  | 1) [[var1]] does not exist in your Data List                                                                                               |
	| [[var]][[var]]                 | true  | 1) Invalid Region [[var]][[var]]                                                                                                           |
	| [[a]].[[b]]                    | true  | 1) Invalid Region [[var]][[var]]                                                                                                           |
	| [[[[a]].[[b]]]]                | true  | 1) Invalid Region [[var]][[var]]                                                                                                           |
	| [[[[a]].[[b]]]]cd]]            | true  | 1.Invalid syntax - You have close (}}) without related open ( [[) 2.Vaiable - Invalid expression: opening and closing brackets don't match |
	| [[var*]]                       | true  | 1) [[var*]] does not exist in your Data List                                                                                               |
	| [[1var]]                       | true  | 1) Invalid expression: cant start a variable name with a number                                                                            |
	| [[@var]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[#var]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[$var]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[%var]]                       | true  | 1)Variable name contains invalid character(s)                                                                                              |
	| [[^var]]                       | true  | 1)Variable name contains invalid character(s)                                                                                              |
	| [[&var]]                       | true  | 1)Variable name contains invalid character(s)                                                                                              |
	| [[*var]]                       | true  | 1)Variable name contains invalid character(s)                                                                                              |
	| [[(var]]                       | true  | 1)Variable name contains invalid character(s)                                                                                              |
	| [[var]](var)]]                 | true  | 1)Vaiable - Invalid expression: opening and closing brackets don't match                                                                   |
	| [[abcdefghijklmnopqrstuvwxyz]] | false |                                                                                                                                            |
	| [[var.]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var,]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var/]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var:]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var"]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var']]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var;]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var?]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[var 1]]                      | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[:var 1]]                     | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[,var]]                       | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[test,var]]                   | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[test. var]]                  | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[test.var]]                   | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[test. 1]]                    | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[test.1]]                     | true  | 1) Variable name contains invalid character(s)                                                                                             |
	| [[test. *]]                    | true  | 1) Variable name contains invalid character(s)                                                                                             |







Scenario Outline: Variable with Recordset language parsor 
	Given I have a variable '<variable>'	
    When I validate
	Then has error will be '<error>'.
	And the error message will be '<message>'
Examples: 	
	| variable                | error | message                                                          |
	| [[rec().a]]             | false |                                                                  |
	| [[rec(1).a]]            | false |                                                                  |
	| [[rec(*).a]]            | false |                                                                  |
	| [[rec(*).&]]            | true  | 1) Variable name contains invalid character(s)                   |
	| [[fok(),a]]             | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec()*a]]             | true  | 1) Variable name contains invalid character(s)                   |
	| [[mar()&a]]             | true  | 1) Variable name contains invalid character(s)                   |
	| [[mar()&a]]             | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec()!a]]             | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec()@a]]             | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec()(a]]             | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec()%`a]]            | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec()         a]]     | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec(1)]]              | true  | 1) Variable name contains invalid character(s)                   |
	| [[rec(1).[[zar().1]]]]  | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[rec(a).[[zar().a]]]]  | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[rec().[[b]]]]         | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[{{rec(_).a}}]]]       | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[*[{{rec(_).a}}]]]     | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[rec(23).[[var}]]]]    | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[rec(23).[[var}]]]]    | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[rec(23).[[var*]]]]    | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[rec(23).[[var%^&%]]]] | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
	| [[rec().a]]234234]]     | true  | 1) Invalid syntax - You have close (]]) without related open([[) |
						
