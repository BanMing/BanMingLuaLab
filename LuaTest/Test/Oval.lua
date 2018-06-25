Oval = {}
function Oval.IsInOval(a, b, x, y)
    return (math.pow(x,2) / math.pow(a,2)) + (math.pow(y,2) / math.pow(b,2)) < 1
end

function Oval.RandomPointOval(a, b)
    local x = a
    local y = b
    local func = function()
        x = math.random(0, a)
        y = math.random(0, b)
        local tempX = math.random(2)
        local tempY = math.random(2)
        x = tempX == 1 and x or (-x)
        y = tempY == 1 and y or (-y)
    end
    func()
    while not Oval.IsInOval(a, b, x, y) do
        func()
    end
    return x, y
end

print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))
print(Oval.RandomPointOval(2, 3))


