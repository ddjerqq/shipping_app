add-migration:
	dotnet ef migrations add --project ./src/Persistence/Persistence.csproj --startup-project ./src/Presentation/Presentation.csproj --context Persistence.AppDbContext --configuration Debug --verbose $(name) --output-dir Migrations

apply-migrations:
	dotnet ef database update --project ./src/Persistence/Persistence.csproj --startup-project ./src/Presentation/Presentation.csproj --context Persistence.AppDbContext --configuration Debug --verbose

docker-build:
	echo "building for $(arch)"
	docker build -t ddjerqq/shippingapp:latest-$(arch) --progress=plain --build-arg TARGETARCH=$(arch) .