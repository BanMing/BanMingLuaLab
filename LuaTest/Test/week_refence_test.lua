-- week_refence_test.lua
-- @Author      : BanMing
-- @Date        : 4/9/2020, 7:30:10 AM
-- @Description :弱引用

t = {}
-- 设置 table t的key值为弱引用类型 k
-- kv 为key与value
setmetatable(t, {__mode = "k"})

-- 使用一个table作为t的key值
key1 = {name = "key1"}
t[key1] = 1
key1 = nil

-- 又使用一个table作为t的key值
key2 = {name = "key2"}
t[key2] = 1
key2 = nil

-- 使用一个table作为t的value

value = {name = "value"}
t[2] = value
value = nil

-- 强行进行一次垃圾收集
collectgarbage()

-- 不设置元方法 输出：
-- table: 0x7ffe79e03390	1	
-- 2	table: 0x7ffe79e03450	
-- table: 0x7ffe79e03210	1

-- 设置__mode = "k"方法 输出：
-- 2	table: 0x7ff8aec30540

-- 设置 __mode = "kv"方法 无输出
for k, v in pairs(t) do
    print(k, v)
end
