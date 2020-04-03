-- metatable_test.lua
-- @Author      : BanMing 
-- @Date        : 4/3/2019, 6:50:47 PM
-- @Description : 

local setmetatable = setmetatable

local mytable = {}
local mymetatable = {}
local callFunc = function()
    print("$$$$$$$$$$$$$$$$$$")
end
setmetatable(
    mytable,
    {
        -- 查找元方法
        __index = mymetatable,
        -- 直接调用元方法
        __call = callFunc
    }
)

-- local mytable = setmetatable({}, {})
-- mytable.fun1 = function()
--     print("@@@@@@@@@@@@")
-- end

mymetatable.fun1 = function()
    print("##############")
end

mytable.fun1()
mytable()