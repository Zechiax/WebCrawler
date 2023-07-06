import React from "react";
import { DataSet, Network } from "vis-network/standalone/esm/vis-network";
import { useLocation } from "react-router";
import "./ViewGraphNG.css";
import { ProgressBar } from 'react-bootstrap';

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
  graphsIds: number[];
  stabilizationProgress: number;
  errorMessage?: string;
  intervalId?: NodeJS.Timeout;
  liveGraphUrlBase: GraphView;
}

enum GraphView {
  Domain ="Record/livegraph/domains/",
  Website = "Record/livegraph/websites/",
}

class ViewGraphNGInternal extends React.Component<
    { graphsIds: number[] },
    IState
> {
  private graphRef = React.createRef<HTMLDivElement>();
  private network?: Network;
  private nodes = new DataSet<INode>();
  private edges = new DataSet<IEdge>();

  constructor(props: { graphsIds: [] }) {
    super(props);
    this.state = {
      graphsIds: [...this.props.graphsIds],
      stabilizationProgress: 0,
      errorMessage: undefined,
      intervalId: undefined,
      liveGraphUrlBase: GraphView.Domain,
    };
  }

  async componentDidMount() {
    console.log("Component mounted, loading graphs with ids: " + this.state.graphsIds);
    await this.updateGraphAsync().then(r => {
      console.log("Graph data loaded");
      if (!this.state.errorMessage) {
        this.initializeGraph();
        console.log("Graph initialized");
      }
    });

    console.log("Starting periodic graph update");
    // Start the interval timer to update the graph every 5 seconds
    this.startLiveUpdate();
  }

  async componentDidUpdate(prevProps: {}, prevState: IState) {
    if (prevState.liveGraphUrlBase !== this.state.liveGraphUrlBase) {
      this.removeGraph();
      this.stopLiveUpdate();
      await this.updateGraphAsync();
      this.initializeGraph();
      this.startLiveUpdate();
    }
  }

  async updateGraphAsync() {
    console.log(this.state.graphsIds);
    const graphsJson = [];

    for (const id of this.state.graphsIds) {
      const response = await fetch(this.state.liveGraphUrlBase + id);

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

    this.nodes.update(graphData.nodes);
    this.edges.update(graphData.edges);
  }

  showErrorMessage() {
    console.log("Showing error message");
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

        let numberOfNeighbours = node.Neighbours.length;

        if (numberOfNeighbours <= 0) {
          numberOfNeighbours = 1;
        }

        if (alreadyPresentNode) {
          if (
              Date.parse(recordForGraph.crawlInfo.lastExecution.started) >
              Date.parse(alreadyPresentNode.started)
          ) {
            alreadyPresentNode.label = node.Title;
            alreadyPresentNode.color = color;
            alreadyPresentNode.title = element;
            alreadyPresentNode.value = numberOfNeighbours;
            alreadyPresentNode.mass = numberOfNeighbours;
          }
        } else {
          graphData.nodes.push({
            id: node.Url,
            label: node.Title,
            color: color,
            started: recordForGraph.crawlInfo.lastExecution.started,
            title: element,
            value: numberOfNeighbours,
            mass: numberOfNeighbours,
          });
        }

        for (const neighbourUrl of node.Neighbours) {
          // Ids are from url to url
          const id = `${node.Url} -> ${neighbourUrl}`;
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

  removeGraph() {
    console.log("Removing graph");
    if (this.network) {
      this.network.destroy();
    }
  }

  startLiveUpdate() {
    console.log("Starting interval");
    const intervalId = setInterval(async () => {
      await this.updateGraphAsync();
    }, 5000);

    // Store the intervalId in the state
    this.setState({ intervalId });
  }

  stopLiveUpdate() {
    console.log("Stopping interval");
    // Clear the interval right before component unmount
    if (this.state.intervalId) {
      clearInterval(this.state.intervalId);
    }
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
    console.log("Component unmounting, clearing interval");
    // Clear the interval right before component unmount
    this.stopLiveUpdate();

    this.removeGraph();
  }

  initializeGraph() {
    // provide the data in the vis format
    const data = {
      nodes: this.nodes,
      edges: this.edges,
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
    this.network = new Network(this.graphRef.current, data, options);

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

    // We show progress bar if the graph is not yet stabilized
    // if (this.state.stabilizationProgress < 100) {
    //   return (
    //       // We use bootstrap progress bar
    //       <div className="progress">
    //         <ProgressBar
    //             striped
    //             variant="info"
    //             now={this.state.stabilizationProgress}
    //             label={`${this.state.stabilizationProgress}%`}
    //         />
    //       </div>
    //   );
    // }

    return (
        <>
          <button onClick={async () => {
            await this.updateGraphAsync();
          }}
          >Test
          </button>
          <div ref={this.graphRef} className="full-height"/>
        </>
    );
  }


}

export default ViewGraphNG;
