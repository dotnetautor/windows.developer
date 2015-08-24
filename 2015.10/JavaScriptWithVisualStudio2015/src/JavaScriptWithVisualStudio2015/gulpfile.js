/// <binding BeforeBuild='build' />


var gulp = require('gulp');
var streamqueue = require('streamqueue');

var dest = "./wwwroot/";
var plugins = require("gulp-load-plugins")({
  pattern: ['gulp-*', 'gulp.*', 'main-bower-files'],
  replaceString: /\bgulp[\-.]/
});

gulp.task('jsLibs', function() {
  gulp.src(plugins.mainBowerFiles().concat(['ClientApp/**/*.js']))
		.pipe(plugins.filter('*.js'))
    .pipe(plugins.order([
      "jquery.js",
      "bootstrap.js",
      "angular.js",
      "*"
    ]))
		.pipe(plugins.concat('vendor.js'))
		.pipe(plugins.uglify())
		.pipe(gulp.dest(dest + 'js'));
});

gulp.task('cssLibs', function() {
  var files = gulp.src(plugins.mainBowerFiles().concat(['ClientApp/css/*']))
  var cssFiles = files.pipe(plugins.filter('*.css'));
  var lessFiles = files.pipe(plugins.filter('*.less')).pipe(plugins.less());
  return streamqueue({ objectMode: true }, cssFiles, lessFiles)
		.pipe(plugins.concat('vendor.css'))
		.pipe(plugins.minifyCss())
		.pipe(gulp.dest(dest + 'css'));
});

gulp.task("build", ["jsLibs","cssLibs"], function () {

});

gulp.task('default', ['build'], function () {
    // place code for your default task here
});
