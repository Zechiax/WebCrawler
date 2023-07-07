import React from "react";
import { DataSet, Network } from "vis-network/standalone/esm/vis-network";
import { useLocation } from "react-router";
import Button from "react-bootstrap/Button";
import "./ViewGraphNG.css";
//import { ProgressBar } from 'react-bootstrap';

export const ViewGraphNG = (props: any) => {
  const location = useLocation();
  return (
    <ViewGraphNGInternal graphsIds={location.state.graphsIds} {...props} />
  );
};

interface INode {
  id: string;
  label: string;
  color: string;
  started?: string;
  fixed?: {
    x: boolean;
    y: boolean;
  };
  value?: number;
  title?: string | HTMLElement;
  mass?: number;
}

interface IEdge {
  id: string;
  from: string;
  to: string;
}

interface IGraphData {
  nodes: INode[];
  edges: IEdge[];
}

interface IState {
  graphData: IGraphData;
  graphsIds: number[];
  stabilizationProgress: number;
  errorMessage?: string;
  staticView: boolean;
}

class ViewGraphNGInternal extends React.Component<
  { graphsIds: number[] },
  IState
> {
  private graphRef = React.createRef<HTMLDivElement>();
  private network?: Network;

  constructor(props: { graphsIds: [] }) {
    super(props);
    this.state = {
      graphData: {
        nodes: [],
        edges: [],
      },
      graphsIds: [...this.props.graphsIds],
      stabilizationProgress: 0,
      errorMessage: undefined,
      staticView: false
    };
  }

  componentDidMount() {
    this.getGraphAsync("Record/livegraph/domains/");
  }

  componentDidUpdate(prevProps: {}, prevState: IState) {
    if (prevState.graphData !== this.state.graphData) {
      this.renderGraph();
    }
  }

  async getGraphAsync(urlbase: string) {
    console.log(this.state.graphsIds);
    const graphsJson = [];

    for (const id of this.state.graphsIds) {
      const response = await fetch(urlbase + id);

      if (response.ok) {
        const graphJson = await response.json();
        graphJson["websiteRecordId"] = id;
        graphsJson.push(graphJson);
      } else {
        console.log("Error fetching graph: " + id);
        this.showErrorMessage();
        return;
      }
    }

    const graphData = await this.convertGraphsJsonToVisJsonAsync(graphsJson);
    this.setState({ graphData });
  }

  showErrorMessage() {
    this.setState({
      errorMessage: "The graph for this record could not be fetched. Please try again later.",
    });
  }

  async convertGraphsJsonToVisJsonAsync(
    graphsJson: any[]
  ): Promise<IGraphData> {
    const graphData: IGraphData = {
      nodes: [],
      edges: [],
    };

    for (const graphJson of graphsJson) {
      console.log("Fetching record for graph: " + graphJson.websiteRecordId);
      const recordForGraphResponse = await (
        await fetch(`/Record/${graphJson.websiteRecordId}`)
      );

      if (!recordForGraphResponse.ok) {
        console.log(
          "Error fetching record for graph: " + graphJson.websiteRecordId
        );
        this.showErrorMessage();
        return;
      }

      const recordForGraph = await recordForGraphResponse.json();

      for (const node of graphJson.Graph) {
        const color = new RegExp(
          recordForGraph.crawlInfo.regexPattern
        ).test(node.Url)
          ? this.stringToColour(recordForGraph.label)
          : "orange";

        const alreadyPresentNode = graphData.nodes.find(
          (n) => n.id === node.Url
        );

        let element = node.Url;

        if (alreadyPresentNode) {
          if (
            Date.parse(recordForGraph.crawlInfo.lastExecution.started) >
            Date.parse(alreadyPresentNode.started)
          ) {
            alreadyPresentNode.label = node.Title;
            alreadyPresentNode.color = color;
            alreadyPresentNode.title = element;
          }
        } else {
          graphData.nodes.push({
            id: node.Url,
            label: node.Title,
            color: color,
            started: recordForGraph.crawlInfo.lastExecution.started,
            title: element,
          });
        }

        for (const neighbourUrl of node.Neighbours) {
          // We generate random ids for the edges
          const id = Math.random().toString(36).substr(2, 9);
          graphData.edges.push({
            id: `${id}`,
            from: node.Url,
            to: neighbourUrl,
          });
        }
      }
    }

    return graphData;
  }

  stringToColour = (str: string) => {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      // tslint:disable-next-line:no-bitwise
      hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    let colour = "#";
    for (let i = 0; i < 3; i++) {
      // tslint:disable-next-line:no-bitwise
      const value = (hash >> (i * 8)) & 0xff;
      colour += ("00" + value.toString(16)).substr(-2);
    }
    return colour;
  };

  componentWillUnmount() {
    if (this.network) {
      this.network.destroy();
    }
  }

  renderGraph() {
    // create an array with nodes
    const nodes = new DataSet<INode>(this.state.graphData.nodes);

    // create an array with edges
    const edges = new DataSet<IEdge>(this.state.graphData.edges);

    // provide the data in the vis format
    const data = {
      nodes: nodes,
      edges: edges,
    };

    const options = {
      physics: {
        stabilization: {
          enabled: true,
          iterations: 1000,
        },
        barnesHut: {
          gravitationalConstant: -80000,
          springConstant: 0.001,
          springLength: 200,
        },
        timestep: 2.5,
        adaptiveTimestep: true,
      },
      layout: {
        improvedLayout: false,
        hierarchical: false,
      },
      edges: {
        smooth: false,
        color: {
          color: "#E6E5E6",
          inherit: false,
        },
      },
      nodes: {
        shape: "dot",
        scaling: {
          min: 100,
          max: 500,
        },
        font: {
          size: 12,
          face: "Tahoma",
        },
      },
      interaction: {
        hover: true,
        tooltipDelay: 200,
        hideEdgesOnDrag: true,
        hideEdgesOnZoom: true,
      },
      height: "100%",
    };

    // initialize your network!
    this.network = new Network(this.graphRef.current!, data, options);

    //Adjust node size based on the number of connected edges
    const nodeDegrees = new Map<string, number>();
    nodes.forEach((node: INode) => {
      nodeDegrees.set(node.id, this.network!.getConnectedEdges(node.id).length);
    });

    nodes.forEach((node: INode) => {
      const degree = nodeDegrees.get(node.id);
      if (degree !== undefined) {
        node.value = degree;
        node.mass = degree;
        nodes.update(node);
      }
    });

    this.network.on("stabilizationProgress", (params) => {
      this.setState({
        stabilizationProgress: (params.iterations / params.total) * 100,
      });
    });

    this.network.on("stabilizationIterationsDone", () => {
      this.setState({ stabilizationProgress: 100 });
      this.network!.fit();
    });
  }

  render() {
    if (this.state.errorMessage) {
      return (
          // Possible style style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}
          <div  className="alert alert-danger">
            <strong>{this.state.errorMessage}</strong>
          </div>
      );
    }

    return (
        <>
            <div ref={this.graphRef} className="full-height" />

            <div
                style={{
                    display: "grid",
                    gridTemplateAreas: "'top''middle''bottom'",
                    position: "absolute",
                    top: "70px",
                    right: "10px",
                    gap: "10px",
                }}
            >
                <Button
                    style={{
                        gridArea: "top",
                    }}
                >
                    "Websites view X Domains view"
                </Button>
                <Button
                    style={{
                        gridArea: "middle",
                    }}
                    onClick={() => {
                        this.setState({ staticView: !this.state.staticView });
                    }}

                >
                    { this.state.staticView ? "Dynamic View" : "Static View" }
                </Button>
                <Button
                    style={{
                        gridArea: "bottom",
                        visibility: this.state.staticView ? "visible" : "hidden",
                    }}
                >
                    "Update Graph"
                </Button>

            </div>
        </>
    );
  }


}

export default ViewGraphNG;
