﻿// Make this available to chrome debugger
//@ sourceURL=WebServiceViewModel.js  

function WebServiceViewModel(saveContainerID, resourceID, sourceName, environment, resourcePath) {
    var self = this;
    var SRC_URL = 0;
    var SRC_BODY = 1;
    
    var $tabs = $("#tabs");
    var $sourceAddress = $("#sourceAddress");
    var $requestUrl = $("#requestUrl");
    var $requestBody = $("#requestBody");
    var $addResponseDialog = $("#addResponseDialog");
    
    $("#addResponseButton")
      .text("")
      .append('<img height="16px" width="16px" src="images/edit.png" />')
      .button();
    
    self.$webSourceDialogContainer = $("#webSourceDialogContainer");

    self.currentEnvironment = ko.observable(environment); //2013.06.08: Ashley Lewis for PBI 9458 - Show server
    self.titleSearchString = "Web Service";
    
    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    self.onLoadSourceCompleted = null;
    self.inputMappingLink = "Please enter a request url or body first (Step 3 & 4)";
    self.outputMappingLink = "Please run a test first (Step 5) or paste a response first (Step 6)";
    
    self.data = new ServiceData(self.isEditing ? resourceID : $.Guid.Empty(), "WebService");
    self.data.requestUrl = ko.observable("");
    self.data.requestMethod = ko.observable("");
    self.data.requestHeaders = ko.observable("");
    self.data.requestBody = ko.observable("");
    self.data.requestResponse = ko.observable("");   

    self.sourceAddress = ko.observable("");
    
    self.placeHolderRequestBody = ko.computed(function() {
        return self.data.source() ? "" : "e.g. CountryName=[[CountryName]]";
    });
    self.placeHolderRequestUrl = ko.computed(function () {
        return self.data.source() ? "" : "e.g. http://www.webservicex.net/globalweather.asmx/GetCitiesByCountry?CountryName=[[CountryName]]";
    });
    
    $requestUrl.keydown(function (e) {
        self.isBackspacePressed = e.keyCode == 8;
        self.isEqualPressed = e.keyCode == 187;
        self.isCloseBracketPressed = e.keyCode == 221;
    }).keyup(function (e) {
        self.isBackspacePressed = false;
        self.isEqualPressed = false;
        self.isCloseBracketPressed = false;
    });

    $requestBody.keydown(function (e) {
        self.isCloseBracketPressed = e.keyCode == 221;
    }).keyup(function (e) {
        self.isCloseBracketPressed = false;
    });
    
    self.isUpdatingVariables = false;
    self.isBackspacePressed = false;
    self.isEqualPressed = false;
    self.isCloseBracketPressed = false;
    self.hasSourceSelectionChanged = false;

    self.pushRequestVariable = function(varName, varSrc, varValue) {
        var oldVar = $.grep(self.data.method.Parameters(), function (e) { return e.Name == varName; });
        if (oldVar.length == 0) {
            self.data.method.Parameters.push({ Name: varName, Src: varSrc, Value: varValue, DefaultValue: varValue, IsRequired: false, EmptyToNull: false });
        }
    };
    
    self.getOutputDisplayName = function (name) {
        return name && name.length > 20 ? ("..." + name.substr(name.length - 17, name.length)) : name;
    };
    
    self.pushRecordsets = function (result) {
        var hasResults = result.Recordsets && result.Recordsets.length > 0;
        self.hasTestResults(hasResults);

        recordsets.pushResult(self.data.recordsets, result.Recordsets);
    };

    self.getParameter = function (text, start) {
        var result = { name: "", value: "" , nameStart: 0, nameEnd: 0, valueStart: 0, valueEnd: 0 };

        result.nameEnd = start - 1;
        result.nameStart = text.lastIndexOf("&", result.nameEnd);
        if (result.nameStart == -1) {
            result.nameStart = text.lastIndexOf("?", result.nameEnd);
        }
        if (result.nameStart != -1 && result.nameEnd >= result.nameStart) {
            result.nameStart++;
            result.name = text.substring(result.nameStart, result.nameEnd);
        }
        
        result.valueStart = start;
        result.valueEnd = text.indexOf("&", start);
        if (result.valueEnd == -1) {
            result.valueEnd = text.indexOf("=", start - 1);
            if (result.valueEnd == -1) {
                result.valueEnd = text.length;
            } else {
                result.valueEnd = result.valueStart;
            }
        }
        result.value = text.substring(result.valueStart, result.valueEnd);
        
        return result;
    };

    self.extractAndPushRequestVariable = function (text, start, varSrc) {
        var prev = text.slice(start - 2, start - 1);
        if (prev == "]") {
            var idx = text.lastIndexOf("[[", start);
            if (idx != -1) {
                var paramName = text.substring(idx + 2, start - 2);
                self.pushRequestVariable(paramName, varSrc, "");
            }
        }
    };

    self.updateVariablesText = function (varSrc, newValue, caretPos) {
        try {
            self.isUpdatingVariables = true;
            if (varSrc == SRC_URL) {
                self.data.requestUrl(newValue);
                $requestUrl.caret(caretPos);

            } else {
                self.data.requestBody(newValue);
            }
        } finally {
            self.isUpdatingVariables = false;
        }
    };
    
    self.updateAllVariables = function (varSrc, newValue) {
        if (!newValue) {
            newValue = "";
        }

        var paramNames = [];
        var paramValues = [];

        var paramVars = newValue.match(/\[\[\w*\]\]/g); // match our variables!
        if (!paramVars) {
            paramVars = [];
        }
        if (varSrc == SRC_URL) {
            // regex should include a lookbehind to exclude leading char but lookbehind ?<= is not supported!            
            paramNames = newValue.match(/([?&#]).*?(?==)/g); // ideal regex: (?<=[?&#]).*?(?==) 
            paramValues = newValue.match(/=([^&#]*)/g); // ideal regex: (?<==)([^&#]*) 
            if (!paramNames) {
                paramNames = [];
            }
            if (!paramValues) {
                paramValues = [];
            }
            paramNames = paramNames.concat(paramVars);
            paramValues = paramValues.concat(new Array(paramVars.length));
        } else {
            paramNames = paramVars;
            paramValues = new Array(paramNames.length);
        }

        $.each(paramNames, function (index, paramName) {
            var varName = paramName;
            if (varSrc == SRC_URL) {
                if (paramName.substr(0, 2) != "[[") {
                    paramName = paramName ? paramName.slice(1) : "";
                    varName = "[[" + paramName + "]]";
                } else {
                    paramName = paramName.slice(2).slice(0, paramName.length - 4);
                }
            } else {
                paramName = paramName.slice(2).slice(0, paramName.length - 4);
            }
            var paramValue = paramValues[index];

            self.pushRequestVariable(paramName, varSrc, paramValue ? paramValue.slice(1) : ""); // remove = prefix from paramValue

            if (varSrc == SRC_URL) {
                var replaceStr = paramName + "=" + varName;
                if (newValue.indexOf(replaceStr) == -1) {
                    newValue = newValue.replace(paramName + paramValue, paramName + "=" + varName);
                }
            }
            self.updateVariablesText(varSrc, newValue, newValue.length);
            return true;
        });
    };


    self.updateVariables = function (varSrc, newValue) {
        var start = varSrc == SRC_URL ? $requestUrl.caret() : $requestBody.caret();

        if (self.hasSourceSelectionChanged) {
            self.updateAllVariables(varSrc, newValue);
            return;
        }
        if (varSrc == SRC_URL) {
            if (self.isEqualPressed) {
                var param = self.getParameter(newValue, start);

                self.pushRequestVariable(param.name, varSrc, param.value);

                var prefix = newValue.slice(0, param.valueStart);
                var postfix = newValue.slice(param.valueEnd, newValue.length);
                var paramValue = "[[" + param.name + "]]";
                newValue = prefix.concat(paramValue).concat(postfix);

                self.updateVariablesText(varSrc, newValue, start + paramValue.length);

            } else if (self.isCloseBracketPressed) {
                self.extractAndPushRequestVariable(newValue, start, varSrc);
            }
        } else { // SRC_BODY
            if (self.isCloseBracketPressed) {
                self.extractAndPushRequestVariable(newValue, start, varSrc);
            }
        }

        // Clean up variables
        if (newValue) {
            var paramVars = newValue.match(/\[\[\w*\]\]/g);    // match our variables!
            if (paramVars) {
                for (var i = self.data.method.Parameters().length - 1; i >= 0; i--) {
                    var requestVar = self.data.method.Parameters()[i];
                    var paramVar = $.grep(paramVars, function (e) { return e == "[[" + requestVar.Name + "]]"; });
                    if (paramVar.length == 0 && requestVar.Src == varSrc) {
                        self.data.method.Parameters.splice(i, 1);
                    }
                }
            }
        }
        
        self.data.method.Parameters.sort(utils.nameCaseInsensitiveSort);
    };
    
    self.data.requestUrl.subscribe(function (newValue) {
        if (!self.isUpdatingVariables) {            
            self.updateVariables(SRC_URL, newValue);
        }
    });
    
    self.data.requestBody.subscribe(function (newValue) {
        if (!self.isUpdatingVariables) {
            self.updateVariables(SRC_BODY, newValue);
        }
    });

    self.requestMethods = ko.observableArray(["GET", "POST", "PUT", "DELETE", "TRACE"]);  
    
    self.sources = ko.observableArray();
    self.upsertSources = function (result) {
        var id = result.ResourceID.toLowerCase();
        var name = result.ResourceName.toLowerCase();
        var idx = -1;
        var replace = false;
        $.each(self.sources(), function (index, source) {
            if (source.ResourceID.toLowerCase() === id) {
                idx = index;
                replace = true;
                return false;
            }           
            if (idx == -1 && name < source.ResourceName.toLowerCase()) {
                idx = index;
            }
            return true;
        });
        
        if (idx != -1) {
            if (replace) {
                self.sources()[idx] = result;
            } else {
                self.sources.splice(idx, 0, result);
            }
        } else {
            self.sources.push(result);
        }
    };
    
    self.data.source.subscribe(function (newValue) {             
        // our sources is a list of Resource's and NOT WebSource's 
        // so we have to load it
        if (newValue && !newValue.Address) {
            self.loadSource(newValue.ResourceID);
            return;
        }
        self.onSourceChanged(newValue);
    });

    self.onSourceChanged = function (newValue) {
        self.hasSourceSelectionChanged = true;
        try {
            self.data.requestBody("");
            self.data.requestResponse("");
            self.data.method.Parameters.removeAll();
            self.data.recordsets.removeAll();
            self.data.requestUrl(newValue ? newValue.DefaultQuery : ""); // triggers a call updateVariables()
            self.hasTestResults(false);
            self.sourceAddress(newValue ? newValue.Address : "");

            var addressWidth = 0;
            if (newValue) {
                $sourceAddress.css("display", "inline-block");
                addressWidth = $sourceAddress.width() + 1;
                $sourceAddress.css("display", "table-cell");
            }
            $sourceAddress.css("width", addressWidth);
        } finally {
            self.hasSourceSelectionChanged = false;
        }
    };

    self.selectSourceByID = function (theID) {
        theID = theID.toLowerCase();
        var found = false;
        $.each(self.sources(), function (index, source) {
            if (source.ResourceID.toLowerCase() === theID) {
                found = true;
                self.data.source(source);
                return false;
            }
            return true;
        });
        return found;
    };

    self.selectSourceByName = function (theName) {
        var found = false;
        if (theName) {
            theName = theName.toLowerCase();
            $.each(self.sources(), function (index, source) {
                if (source.ResourceName.toLowerCase() === theName) {
                    found = true;
                    self.data.source(source);
                    return false;
                }
                return true;
            });
        }
        return found;
    };
    
    self.hasTestResults = ko.observable(false);
    
    self.hasInputs = ko.computed(function() {
        return self.data.method.Parameters().length > 0;
    });
    
    self.isFormValid = ko.computed(function () {
        return self.hasTestResults();
    });
    
    self.isTestVisible = ko.observable(true);
    self.isTestEnabled = ko.computed(function () {
        return self.data.source() ? true : false;
    });
    self.isTestResultsLoading = ko.observable(false);
    self.isEditSourceEnabled = ko.computed(function () {
        return self.data.source();
    });

    self.isRequestBodyRequired = ko.computed(function () {
        return self.data.requestMethod() != "GET" && self.isTestEnabled();
    });

    self.title = ko.observable("New Service");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });
    self.saveTitle = ko.computed(function () {
        return "<b>" + self.title() + "</b>";
    });

    $tabs.on("tabsactivate", function (event, ui) {
        var idx = $tabs.tabs("option", "active");
        self.isTestVisible(idx == 0);
    });

    self.showTab = function (tabIndex) {
        $tabs.tabs("option", "active", tabIndex);
    };

    self.getJsonData = function () {
        return ko.toJSON(self.data);
    };

    self.load = function () {
        self.loadSources(
            self.loadService());
    };
    
    self.loadService = function () {
        var args = ko.toJSON({
            resourceID: resourceID,
            resourceType: "WebService"
        });
        
        $.post("Service/WebServices/Get" + window.location.search, args, function (result) {
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            if (!result.ResourcePath && resourcePath) {
                self.data.resourcePath(resourcePath);
            }

            // This will be invoked by loadSource 
            self.onLoadSourceCompleted = function() {
                if (result.Method) {
                    self.data.method.Name(result.Method.Name);
                    self.data.method.Parameters(result.Method.Parameters);
                }

                self.data.requestUrl(result.RequestUrl);
                self.data.requestMethod(result.RequestMethod);
                self.data.requestHeaders(result.RequestHeaders);
                self.data.requestBody(result.RequestBody);
                self.data.requestResponse(result.RequestResponse);

                self.pushRecordsets(result);
                self.onLoadSourceCompleted = null;
            };
            
            var found = sourceName && self.selectSourceByName(sourceName);           
            if (!found) {
                found = utils.IsNullOrEmptyGuid(result.Source.ResourceID)
                    ? self.selectSourceByName(result.Source.ResourceName)
                    : self.selectSourceByID(result.Source.ResourceID);
            }
            
            // MUST set these AFTER setting data.source otherwise they will be blanked!
            if (!found) {
                self.onLoadSourceCompleted();
            }
            self.title(self.isEditing ? "Edit Web Service - " + result.ResourceName : "New Web Service");
        });
    };

    self.loadSources = function (callback) {
        $.post("Service/Resources/Sources" + window.location.search, ko.toJSON({ resourceType: "WebSource" }), function (result) {
            self.sources(result);
            self.sources.sort(utils.resourceNameCaseInsensitiveSort);
        }).done(function () {
            if (callback) {
                callback();
            }
        });
    };
    
    self.loadSource = function (sourceID) {
        $.post("Service/WebSources/Get" + window.location.search, sourceID, function (result) {
            // Need to set this just in case this is the first time!
            self.data.source().Address = result.Address;
            self.data.source().DefaultQuery = result.DefaultQuery;
            self.data.source().AuthenticationType = result.AuthenticationType;
            self.data.source().UserName = result.UserName;
            self.data.source().Password = result.Password;

            self.upsertSources(result);
            self.onSourceChanged(result);
            if (self.onLoadSourceCompleted != null) {
                self.onLoadSourceCompleted();
            }
        });
    };
    
    self.editSource = function () {
        return self.showSource(self.data.source().ResourceName);
    };

    self.newSource = function () {
        return self.showSource("");
    };
    
    self.addResponse = function () {
        $addResponseDialog.dialog("open");
    };
    
    self.testWebService = function (isResponseCleared) {
        self.isTestResultsLoading(true);
        if (isResponseCleared) {
            self.data.requestResponse("");
        }
        
        $.post("Service/WebServices/Test" + window.location.search, self.getJsonData(), function (result) {
            self.isTestResultsLoading(false);
            self.data.requestResponse(result.RequestResponse);
            
            self.pushRecordsets(result);
        });
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.saveViewModel = SaveViewModel.create("Service/WebServices/Save", self, saveContainerID);

    self.save = function () {
        self.saveViewModel.showDialog(true);
    };    

    self.showSource = function (theSourceName) {
        // 
        // webSourceViewModel is a global variable instantiated in WebSource.htm
        //
        webSourceViewModel.showDialog(theSourceName, function (result) {
            self.upsertSources(result);
            self.data.source(result);            
        });
    };
    
    self.load();    
};


WebServiceViewModel.create = function (webServiceContainerID, saveContainerID) {
    $("button").button();
    $("#tabs").tabs();

    var webServiceViewModel = new WebServiceViewModel(saveContainerID, getParameterByName("rid"), getParameterByName("sourceName"), utils.decodeFullStops(getParameterByName("envir")), getParameterByName("path"));

    ko.applyBindings(webServiceViewModel, document.getElementById(webServiceContainerID));
    

    $("#addResponseDialog").dialog({
        resizable: false,
        autoOpen: false,
        modal: true,
        position: utils.getDialogPosition(),
        width: 600,
        buttons: {
            "Ok": function () {
                $(this).dialog("close");
                webServiceViewModel.testWebService(false);
            },
            "Cancel": function () {
                $(this).dialog("close");
            }
        }
    });
    
    // inject WebSourceDialog
    webServiceViewModel.$webSourceDialogContainer.load("Views/Sources/WebSource.htm", function () {
        // 
        // webSourceViewModel is a global variable instantiated in WebSource.htm
        //
        webSourceViewModel.createDialog(webServiceViewModel.$webSourceDialogContainer);
    });
    return webServiceViewModel;
};