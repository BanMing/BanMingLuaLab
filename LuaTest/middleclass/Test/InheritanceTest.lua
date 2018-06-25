local class = require 'middleclass'

Person = class('Person') --this is the same as class('Person', Object) or Object:subclass('Person')
function Person:initialize(name)
  self.name = name
end
function Person:speak()
  print('Hi, I am ' .. self.name ..'.')
end

AgedPerson = class('AgedPerson', Person) -- or Person:subclass('AgedPerson')
AgedPerson.static.ADULT_AGE = 18 --this is a class variable
function AgedPerson:initialize(name, age)
  Person.initialize(self, name) -- this calls the parent's constructor (Person.initialize) on self
  self.age = age
end
function AgedPerson:speak()
  Person.speak(self) -- prints "Hi, I am xx."
  if(self.age < AgedPerson.ADULT_AGE) then --accessing a class variable from an instance method
    print('I am underaged.')
  else
    print('I am an adult.')
  end
end

local p1 = AgedPerson:new('Billy the Kid', 13) -- this is equivalent to AgedPerson('Billy the Kid', 13) - the :new part is implicit
local p2 = AgedPerson:new('Luke Skywalker', 21)
p1:speak()
p2:speak()