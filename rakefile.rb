require 'albacore'
require 'nuget_helper'
task :default => [:build]
dir = File.dirname(__FILE__)

desc "Install missing NuGet packages."
task :restore do
  NugetHelper.exec("restore nunit2.sln")
  NugetHelper.exec("restore nunit3.sln")
end

desc "build solution"
build :build_nunit2 do |msb, args|
  msb.prop :configuration, :Debug
  msb.target = [:Rebuild]
  msb.sln = "nunit2.sln"
end

desc "build solution"
build :build_nunit3 do |msb, args|
  msb.prop :configuration, :Debug
  msb.target = [:Rebuild]
  msb.sln = "nunit3.sln"
end

task :build => [:build_nunit2, :build_nunit3]

desc "test using console"
test_runner :test => [:build] do |runner|
  runner.exe = File.expand_path NugetHelper::nunit_path
  files = Dir.glob(File.join(dir, "**", "bin", "Debug", "*Tests.dll")).map do |n| File.expand_path n end
  runner.files = files 
end
