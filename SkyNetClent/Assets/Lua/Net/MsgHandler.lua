MsgHandler = {}
function MsgHandler.DispatchMsg(bigId, smallId, buffer)
    print("Lua Get Msg BigId:", bigId, "smallId:", smallId)
    local handler = BigMsgHandler[bigId]
    if handler == nil then
        print("handler is NULL!")
    end
    handler:OnMessage(smallId, buffer)
end
BigMsgId = {}
BigMsgHandler = {}
