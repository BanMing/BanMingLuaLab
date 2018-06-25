
--json lua test
require("Test/LuaJson")
local jsonStr="{\"name\":\"BeJson\",\"url\":\"http://www.bejson.com\",\"page\":88,\"isNonProfit\":true,\"isNo\":false,\"address\":{\"street\":\"科技园路.\",\"city\":\"江苏苏州\",\"country\":\"中国\"},\"links\":[{\"name\":\"Google\",\"url\":\"http://www.google.com\"},{\"name\":\"Baidu\",\"url\":\"http://www.baidu.com\"},{\"name\":\"SoSo\",\"url\":\"http://www.SoSo.com\"}],}"
local tesTable= luaJson.json2table(jsonStr)
print (tesTable.name)
local jsonStr=luaJson.table2json(tesTable)
print(jsonStr)