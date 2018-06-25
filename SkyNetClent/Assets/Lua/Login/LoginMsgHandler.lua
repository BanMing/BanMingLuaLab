LoginMsgHandler = class("LoginMsgHandler",BaseHandler)

function LoginMsgHandler:S2CLogin(buffer)
    local msg = Protol.Base_pb.S2CHello()
    msg:ParseFromString(buffer)
    print("randseed:", msg.randseed, "servertime:", msg.servertime)
    self:SendLogin()
end

function LoginMsgHandler:S2CLoginSuccess(buffer)
    local msg = Protol.Base_pb.S2CLoginSuccess()
    msg:ParseFromString(buffer)
    print("id:", msg.id, "name:", msg.name, "servernumber:", msg.servernumber)
end

function LoginMsgHandler:SendLogin()
    local msg = Protol.Base_pb.C2SLogin()
    msg.account = "mm"
    msg.pwd = "pwd"
    msg.servernumber = 33
    msg.mac = "mac"
    local data = msg:SerializeToString()
    TestStart.networkManager:SendMessage(BigMsgId.Login,LoginMsgHandler.C2SMsgId.SendLogin, data)
end

