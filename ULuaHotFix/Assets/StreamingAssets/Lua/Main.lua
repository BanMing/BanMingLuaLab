--主入口函数。从这里开始lua逻辑
function Main()
	math.randomseed(os.time())					
	print("logic start")	 
	print("logic start11")	 
	print("logic start8888888888888888888")	 
	coroutine.start(ImprotFiles)
	-- InitMain()
end

function Test()
	print("testMacMacMacMacMacMacMac~~~~~~~~~~~~Mac")
end
function InitMain()	
	UIWindowFirstLoading.Close()
	local a=MyUnityTool.Find("Canvas/UILayer7/CommonViews")
	if a~=nil then
		a:SetActive(true)
	else
		print("CommonViews is null")
	end
	
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
	
end

function ImprotFiles()

	print("ImprotFiles start1")	
	Test()  
	coroutine.wait(1)
	Test() 
	print("ImprotFiles 2MacMacMacMacMac")	
	coroutine.wait(1)
	InitMain()
end