// Code goes here
(function() {
  describe("my first test collection", function() {
    describe("my first test case", function() {
      it("testing a calculation", function() {
        var x = 4 + 6;
        expect(x).toBe(10);
      });
    });


    describe("my second test case", function() {
      var bar;
      beforeEach(function() {
        this.foo = 42;
        bar = { a : "test", b : 42 }
      });

      it("use `this` to share state", function() {
        expect(this.foo).toEqual(bar.b);
        this.bar = "test pollution?";
      });

      it("prevents test pollution by using 'this'", function() {
        expect(this.foo).toEqual(42);
        expect(this.bar).toBe(undefined);
      });

    });
  });
})()