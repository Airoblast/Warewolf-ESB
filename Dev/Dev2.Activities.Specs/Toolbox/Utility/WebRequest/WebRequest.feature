﻿Feature: WebRequest
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document


Scenario: Enter a URL to download html  
	Given I have the url "http://rsaklfsvrtfsbld/IntegrationTestSite/Proxy.ashx?html"	
	When the web request tool is executed 
	Then the result should contain the string "Welcome to ASP.NET Web API"
	And the web request execution has "NO" error

Scenario: Enter a badly formed URL
	Given I have the url "www.google.comx"	
	When the web request tool is executed 
	Then the result should contain the string ""
	And the web request execution has "AN" error

Scenario: Enter a URL made up of text and variables with no header
    Given I have the url "http://[[site]][[file]]"	
	And I have a web request variable "[[site]]" equal to "rsaklfsvrtfsbld/IntegrationTestSite/"	
	And I have a web request variable "[[file]]" equal to "Proxy.ashx?html"
	When the web request tool is executed 
	Then the result should contain the string "Welcome to ASP.NET Web API"
	And the web request execution has "NO" error

Scenario: Enter a URL and 2 variables each with a header parameter (json)
	Given I have the url "http://rsaklfsvrtfsbld/IntegrationTestSite/Proxy.ashx"	
	And I have a web request variable "[[ContentType]]" equal to "Content-Type"	
	And I have a web request variable "[[Type]]" equal to "application/json"	
	And I have the Header "[[ContentType]]: [[Type]]"
	When the web request tool is executed 
	Then the result should contain the string "["value1","value2"]"
	And the web request execution has "NO" error

Scenario: Enter a URL and 2 variables each with a header parameter (xml)
	Given I have the url "http://rsaklfsvrtfsbld/IntegrationTestSite/Proxy.ashx"	
	And I have a web request variable "[[ContentType]]" equal to "Content-Type"	
	And I have a web request variable "[[Type]]" equal to "application/xml"	
	And I have the Header "[[ContentType]]: [[Type]]"
	When the web request tool is executed 
	Then the result should contain the string "<string>value1</string>"
	And the web request execution has "NO" error

Scenario: Enter a URL that returns json
	Given I have the url "http://rsaklfsvrtfsbld/IntegrationTestSite/Proxy.ashx?json"	
	When the web request tool is executed	
	Then the result should contain the string "["value1","value2"]"
	And the web request execution has "NO" error

Scenario: Enter a URL that returns xml
	Given I have the url "http://rsaklfsvrtfsbld/IntegrationTestSite/Proxy.ashx?xml"
	When the web request tool is executed	
	Then the result should contain the string "<string>value1</string>"
	And the web request execution has "NO" error

Scenario: Enter a blank URL
	Given I have the url ""
	When the web request tool is executed	
	Then the result should contain the string ""
	And the web request execution has "AN" error

Scenario: Enter a URL that returns complex html over https
	Given I have the url "https://www.google.com"	
	When the web request tool is executed 
	Then the result should contain the string "schema.org"
	And the web request execution has "NO" error

Scenario: Enter a URL that is a negative index recordset
	Given I have the url "[[rec(-1).set]]"
	When the web request tool is executed	
	Then the result should contain the string ""
	And the web request execution has "AN" error
