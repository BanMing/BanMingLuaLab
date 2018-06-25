
class=require("middleclass")
require("Net/MsgHandlerHead")
--主入口函数。从这里开始lua逻辑
function Main()
    print("logic start")
    NetFramework.NetworkManager.SetLuaDispatchMsgAction("MsgHandler.DispatchMsg")
end

--场景切换通知
function OnLevelWasLoaded(level)
    collectgarbage("collect")
    Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()

end
