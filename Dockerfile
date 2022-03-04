FROM shimat/ubuntu18-dotnetcore3.1-opencv4.5.0:20201030

ADD ./Publish/ /ANPR 
ENV ASPNETCORE_URLS=http://*:${PORT} 

WORKDIR /ANPR
ENTRYPOINT ["dotnet", "ANPRCV.dll"]