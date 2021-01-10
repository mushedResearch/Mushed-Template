#r "../_lib/Fornax.Core.dll"
#r "nuget: FSharp.Formatting"
#r "nuget: FSharp.Literate"


open System
open System.IO
open FSharp.Formatting.ApiDocs

type ApiPageInfo<'a> = {
    ParentName: string
    ParentUrlName: string
    NamespaceName: string
    NamespaceUrlName: string
    Info: 'a
}

type AssemblyEntities = {
  Label: string
  Modules: ApiPageInfo<ApiDocEntity> list
  Types: ApiPageInfo<ApiDocEntity> list
  GeneratorOutput: ApiDocModel
}

let rec collectModules pn pu nn nu (m: ApiDocEntity) =
    [
        yield { ParentName = pn; ParentUrlName = pu; NamespaceName = nn; NamespaceUrlName = nu; Info =  m}
        yield! m.NestedModules |> List.collect (collectModules m.Name m.UrlName nn nu )
    ]


let loader (projectRoot: string) (siteContent: SiteContents) =
    try
      let dlls =
        [
          "Waypoint", Path.Combine(projectRoot, "..", "build", "Waypoint.dll")
        ]
      let libs =
        [
          Path.Combine (projectRoot, "..", "build")
        ]
      for (label, dll) in dlls do
        let properties = ["project-name", label]
        
        let output = ApiDocs.GenerateModel(dll, markDownComments = true, publicOnly = true, libDirs = libs, parameters = properties)

        let allModules =
            output.AssemblyGroup.Namespaces
            |> List.collect (fun n ->
                List.collect (collectModules n.Name n.Name n.Name n.Name) n.Modules
            )

        let allTypes =
            [
                yield!
                    output.AssemblyGroup.Namespaces
                    |> List.collect (fun n ->
                        n.Types |> List.map (fun t -> {ParentName = n.Name; ParentUrlName = n.Name; NamespaceName = n.Name; NamespaceUrlName = n.Name; Info = t} )
                    )
                yield!
                    allModules
                    |> List.collect (fun n ->
                        n.Info.NestedEntities |> List.map (fun t -> {ParentName = n.Info.Name; ParentUrlName = n.Info.UrlName; NamespaceName = n.NamespaceName; NamespaceUrlName = n.NamespaceUrlName; Info = t}) )
            ]
        let entities = {
          Label = label
          Modules = allModules
          Types = allTypes
          GeneratorOutput = output
        }
        siteContent.Add entities
    with
    | ex ->
      printfn "%A" ex

    siteContent